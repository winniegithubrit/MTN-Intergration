using Microsoft.EntityFrameworkCore;

namespace BankSystem.Data
{
  public class DbContextOptionsFactory
  {
    public static DbContextOptions<ApplicationDbContext> GetDbContextOptions(string connectionString)
    {
      var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
      optionsBuilder.UseMySQL(connectionString);
      return optionsBuilder.Options;
    }
  }
}
