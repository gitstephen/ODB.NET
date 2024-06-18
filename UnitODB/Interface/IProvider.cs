using System.Data;

namespace UnitODB
{
	public interface IProvider
	{
		IDbContext CreateContext();
		IDbConnection CreateConnection(); 
	}
}
