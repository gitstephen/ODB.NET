using System.Data;

namespace UnitODB
{
	public interface IProvider
	{
        IDbContext DbContext { get; set; }
        IDbContext CreateContext();
		IDbConnection CreateConnection(); 
	}
}
