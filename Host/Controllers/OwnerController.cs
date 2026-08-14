using Bogus;
using Host.ContextData;
using Host.DTOS;
using Host.Models;
using Host.Models.Enums;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Host.Controllers;
[Route("api/[controller]")]
[ApiController]
public class OwnerController(ApplicationDbContext context) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;
    [HttpPost("AddOwner")]
    public async Task<IActionResult> AddOwner([FromBody] AddOwnerRequest request)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var isSaved = await AddOwnerRequest(request);
        stopwatch.Stop();
        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        if (isSaved == 0)
        {
            return Ok("No changes was made");
        }
        return Ok($"request take {elapsedMilliseconds}");
    }

    [HttpGet("GetOwners")]
    public async Task<IActionResult> GetOwners([FromQuery] OwnerParameters request)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        if (!request.ValidYearRange)
        {
            return BadRequest("Max year of birth cannot be less than min year of birth");
        }
        var response = GetOwnersPaged(request);
        var metadata = new
        {
            response.TotalCount,
            response.PageSize,
            response.CurrentPage,
            response.TotalPages,
            response.HasNext,
            response.HasPrevious
        };
        stopwatch.Stop();
        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
        return Ok(new
        {
            TimeTaken = elapsedMilliseconds,
            Data = response,
        });
    }

    [HttpGet("TestCryptoGrapy")]
    public async Task<IActionResult> TestCryptoGrapy()
    {
        var result = GenerateOtp();
        return Ok(new
        {
            //TimeTaken = result,
            Data = GenerateRandomNumber(18),
            SpataData = GenerateRandomNumber(18).Count(),
        });
    }

    private static string GenerateOtp()
    {
        using RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        byte[] data = new byte[4];
        rng.GetBytes(data);
        int generatedValue = BitConverter.ToInt32(data, 0);
        // Ensure the value is positive and within the desired range
        int otp = Math.Abs(generatedValue % 1000000);
        return otp.ToString("D6");
    }

    private static string GenerateRandomNumber(int numbersOfDigit)
    {
        byte[] data = new byte[8];
        RandomNumberGenerator.Fill(data);
        long generatedValue = BitConverter.ToInt64(data, 0);
        string digitIdentifier = "1" + new string('0', numbersOfDigit);
        // Ensure the value is positive and within the desired range
        long randomNumber = Math.Abs(generatedValue % long.Parse(digitIdentifier));
        return randomNumber.ToString($"D{numbersOfDigit}");
    }

    private static List<AddOwnerRequest> GenerateOwners(int count)
    {
        var faker = new Faker<AddOwnerRequest>()
            .RuleFor(o => o.Name, f => f.Name.FullName())
            .RuleFor(o => o.DateOfBirth, f => f.Date.Past(50, DateTime.Now.AddYears(-18))) // Ensure owner is at least 18 years old
             .RuleFor(o => o.Gender, f => f.PickRandom<Gender>())
            .RuleFor(o => o.Address, f => f.Address.FullAddress());

        return faker.Generate(count);
    }
    public static string GenerateOtp1()
    {
        var random = new Random(DateTime.Now.Millisecond);
        string Otp = random.Next(100000, 999999).ToString();
        return Otp;
    }

    private async Task<int> AddOwnerRequest(AddOwnerRequest request)
    {
        var owners = GenerateOwners(request.MockCount).Adapt<List<Owner>>();
        await _context.AddRangeAsync(owners);

        /*var model = request.Adapt<Owner>();
        await _context.AddAsync(model);*/
        return await _context.SaveChangesAsync();
    }

    private PagedList<Owner> GetOwnersPaged(OwnerParameters ownerParameters)
    {
        var ownerQuery = _context.Owners
            .Where(o =>
            o.DateOfBirth.Year >= ownerParameters.MinYearOfBirth &&
            o.DateOfBirth.Year <= ownerParameters.MaxYearOfBirth);

        SearchByName(ref ownerQuery, ownerParameters.Name);

        return PagedList<Owner>.ToPagedList(ownerQuery.OrderBy(on => on.Name),
            ownerParameters.PageNumber,
            ownerParameters.PageSize);
    }

    private static void SearchByName(ref IQueryable<Owner> owners, string ownerName)
    {
        if (!owners.Any() || string.IsNullOrWhiteSpace(ownerName))
            return;
        owners = owners.Where(o => o.Name.ToLower().Contains(ownerName.Trim().ToLower()));
    }
}
