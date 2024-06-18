using System;

namespace UnitODB
{
	[Serializable]
	public abstract class OdbEntity : IEntity
	{
		public int Id { get; set; }

		[Ignore]
		public bool ModelState { get; set; }

		public OdbEntity()
		{
			ModelState = true;
		}
	}
}
