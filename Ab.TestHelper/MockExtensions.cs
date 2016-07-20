using Moq;

namespace Ab.TestHelper.Extensions
{

	public static class MockExtensions
	{
		/// <summary>
		/// Sets the <see cref="DefaultValue"/> behavior.
		/// </summary>
		/// <returns>The Mock for chaining.</returns>
		public static Mock<TParameter> DefaultValue<TParameter>(this Mock<TParameter> instance, DefaultValue defaultValue)
			where TParameter : class
		{
			instance.DefaultValue = defaultValue;
			return instance;
		}
	}
}