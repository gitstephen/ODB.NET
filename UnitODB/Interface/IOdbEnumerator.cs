using System;
using System.Collections;
using System.Collections.Generic;

namespace UnitODB
{
	public interface IOdbEnumerator<T> : IEnumerable<T>, IEnumerable, IDisposable
	{
		object GetObject(Type type);
	}
}
