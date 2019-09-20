// ReSharper disable once CheckNamespace
namespace Common.WindowManager
{
	/// <summary>
	/// Результат, возвращаемый из окна при закрытии.
	/// </summary>
	public interface IWindowResult
	{
		/// <summary>
		/// Окно, из которого возвращается результат.
		/// </summary>
		IWindow Window { get; }
	}
}