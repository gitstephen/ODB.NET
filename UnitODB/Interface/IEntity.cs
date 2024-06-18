using System;

namespace UnitODB
{
	public interface IEntity
	{
		int Id { get; }

		bool ModelState { get; set; }
	}
}
