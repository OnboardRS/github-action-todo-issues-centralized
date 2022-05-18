using System.Text;

namespace OnboardRS.ToDoIssues.Business.Models;

public abstract class BaseReflectedToStringObject
{
	public const string ONLY_HERE_FOR_SERIALIZATION_DO_NOT_USE = "This method is only here for serialization and is not intended to be used directly.";

	public override string ToString()
	{
		var sb = new StringBuilder();
		foreach (var property in GetType().GetProperties())
		{
			sb.Append(property.Name);
			sb.Append(": ");
			if (property.GetIndexParameters().Length > 0)
			{
				sb.Append($"[{property.GetIndexParameters().Length}]");
			}
			else
			{
				sb.Append(property.GetValue(this, null));
			}

			sb.Append(" | ");
		}

		return sb.ToString();
	}
}