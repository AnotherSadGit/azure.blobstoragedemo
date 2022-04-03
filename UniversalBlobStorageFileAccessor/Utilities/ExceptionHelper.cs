using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalBlobStorageFileAccessor.Utilities
{
	public static class ExceptionHelper
	{
		/// <summary>
		/// Returns the exception type and message, recursively including inner exceptions.
		/// </summary>
		public static string? GetExceptionDetails(Exception exception)
		{
			if (exception == null)
			{
				return null;
			}

			string message = $"{exception.GetType().Name}: {exception.Message}";

			if (exception.InnerException != null)
			{
				message += "  [Inner Exception - " + GetExceptionDetails(exception.InnerException) + "]";
			}

			return message;
		}

		private static void CheckObjectNotNull<T>(T? objectToTest, string errorMessage) where T : class
		{
			if (objectToTest == null)
			{
				throw new ApplicationException(errorMessage);
			}
		}
	}
}
