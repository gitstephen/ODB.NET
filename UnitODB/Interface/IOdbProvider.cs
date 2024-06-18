using System;

namespace UnitODB
{
	public interface IOdbProvider : IProvider
	{
		string CreateColumn(OdbColumn col); 
		string CreateTable(string name, string[] cols);
		string DropTable(string name);
	}
}
