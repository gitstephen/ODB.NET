using System;
using System.Data;

namespace UnitODB
{
	public interface IOdbProvider : IProvider
	{ 
        string CreateColumn(OdbColumn col);
        IDbDataParameter CreateParameter(int index, object value);
        string CreateTable(string name, string[] cols);
		string DropTable(string name);
	}
}
