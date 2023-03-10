using System;

namespace Universe
{
	[AttributeUsage(AttributeTargets.Class)]
	public class UniverseBuildTaskAttribute : Attribute
	{
		public string Desc;
		public UniverseBuildTaskAttribute(string desc)
		{
			Desc = desc;
		}
	}
}