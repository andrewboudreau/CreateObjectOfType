using Moq;

namespace Ab.TestHelper
{
	class ParameterMock<TParameter> : IParameterMock
		where TParameter : class
	{
		public ParameterMock(Mock<TParameter> value)
		{
			this.Value = value;
		}

		public Mock<TParameter> Value { get; private set; }

		Mock IParameterMock.Mock
		{
			get
			{
				return (Mock)Value;
			}
		}
	}
}
