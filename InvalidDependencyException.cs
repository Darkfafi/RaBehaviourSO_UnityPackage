using System;

namespace RaBehaviourSO
{
	public class InvalidDependencyException : Exception
	{
		public InvalidDependencyException(string message)
			: base(message)
		{

		}
	}
}