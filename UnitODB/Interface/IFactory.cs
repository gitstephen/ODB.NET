using System;

namespace UnitODB
{
	public interface IFactory
	{
		IContainer CreateContainer();
	}
}
