using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ab.TestHelper.UnitTest
{

	[TestClass]
	public class CreateObjectOfTypeTest
	{
		[TestMethod]
		public void WithMocks_NoParameters_ReturnsSomething()
		{
			var target = CreateObjectOfType<TestObject>.WithMocks();
			Assert.IsNotNull(target);
		}

		[TestMethod]
		public void WithMocks_NoParameters_ReturnsNonNullMocks()
		{
			var target = CreateObjectOfType<TestObject>.WithMocks();
			var instance = target.Target as TestObject;

			Assert.IsNotNull(instance);
			Assert.IsNotNull(target.MockOf<ISrvc1>());
			Assert.IsNotNull(target.MockOf<ISrvc2>());
		}

		[TestMethod]
		public void WithMocks_NoParameters_MockBehaviorLoose()
		{
			var target = CreateObjectOfType<TestObject>.WithMocks();

			Assert.AreEqual(MockBehavior.Loose, target.MockOf<ISrvc1>().Behavior);
			Assert.AreEqual(MockBehavior.Loose, target.MockOf<ISrvc2>().Behavior);
		}

		[TestMethod]
		public void WithMocks_NoParameters_DefaultValueEmpty()
		{
			var target = CreateObjectOfType<TestObject>.WithMocks();

			Assert.AreEqual(DefaultValue.Empty, target.MockOf<ISrvc1>().DefaultValue);
			Assert.AreEqual(DefaultValue.Empty, target.MockOf<ISrvc2>().DefaultValue);
		}

		[TestMethod]
		public void WithMocks_OneAlternativeBehavior_MockBehaviorsDiffer()
		{
			var target = CreateObjectOfType<TestObject>.WithMocks(MockBehavior.Strict, typeof(ISrvc2));

			Assert.AreEqual(MockBehavior.Strict, target.MockOf<ISrvc1>().Behavior);
			Assert.AreEqual(MockBehavior.Loose, target.MockOf<ISrvc2>().Behavior);
		}

		[TestMethod]
		public void WithMocks_OneAlternativeBehavior_MochBehaviorsDiffer()
		{
			var target = CreateObjectOfType<TestObject>.WithMocks(MockBehavior.Strict, typeof(ISrvc2));

			Assert.AreEqual(MockBehavior.Strict, target.MockOf<ISrvc1>().Behavior);
			Assert.AreEqual(MockBehavior.Loose, target.MockOf<ISrvc2>().Behavior);
		}

		[TestMethod]
		public void WithMocks_UnknownTypeForAlternative_IsIgnored()
		{
			var target = CreateObjectOfType<TestObject>.WithMocks(MockBehavior.Strict, typeof(GenericParameterHelper));

			Assert.AreEqual(MockBehavior.Strict, target.MockOf<ISrvc1>().Behavior);
			Assert.AreEqual(MockBehavior.Strict, target.MockOf<ISrvc2>().Behavior);
		}

		[TestMethod]
		public void WithMocks_WithTypeForAlternative_UsingTypeParamOverload()
		{
			var target = CreateObjectOfType<TestObject>.WithMocks<ISrvc2>(MockBehavior.Strict);

			Assert.AreEqual(MockBehavior.Strict, target.MockOf<ISrvc1>().Behavior);
			Assert.AreEqual(MockBehavior.Loose, target.MockOf<ISrvc2>().Behavior);
		}

		[TestMethod]
		public void Mock_WithServiceType_ReturnsExpectedMockTypes()
		{
			var target = CreateObjectOfType<TestObject>.WithMocks();
			var expectedStaticType = typeof(Mock<ISrvc1>);
			var expectedInstanceType = new Mock<ISrvc1>().GetType();

			// act
			var actualMockType = target.MockOf<ISrvc1>().GetType();

			// assert
			Assert.AreEqual(expectedStaticType.FullName, actualMockType.FullName);
			Assert.AreEqual(expectedInstanceType.FullName, actualMockType.FullName);
		}

		[TestMethod]
		public void Mock_WithServiceType_ReturnsSameInstance()
		{
			var target = CreateObjectOfType<TestObject>.WithMocks();
			Assert.AreSame(target.MockOf<ISrvc1>().GetType().FullName, target.MockOf<ISrvc1>().GetType().FullName);
		}

		[TestMethod]
		public void Mock_SetupAndVerify()
		{
			var target = CreateObjectOfType<TestObject>.WithMocks();

			target.MockOf<ISrvc1>()
				.Setup(svc => svc.MethodWithStringParameter(It.Is<string>(str => str == "foo")))
				.Verifiable();

			var testObject = target.Target;
			testObject.MethodWithStringParameter("foo");

			target.MockOf<ISrvc1>()
				.Verify(svc => svc.MethodWithStringParameter(It.Is<string>(str => str == "foo")), Times.Once());

		}

		[TestMethod, Description("Moq only allows you to call object once, after all setup is done.")]
		public void Moq_ChangeSetup_TargetReferenceNotReflected()
		{
			var mock = new Mock<ISrvc1>();
			var mock2 = new Mock<ISrvc2>();

			var testObject = new TestObject(mock.Object, mock2.Object);

			mock2.Setup(m => m.Function()).Returns("Blah");

			var result = testObject.Function();

			Assert.AreEqual("foo", result);

		}

		[TestMethod, Description("Moq only allows you to call object once, after all setup is done.")]
		public void Moq_AccessObjectProperty_AddSetup_VerifyObjectIsSetupByAccessingObjectPropertyAgain()
		{
			var mock = new Mock<ISrvc1>();
			var mock2 = new Mock<ISrvc2>();
			var testObject = new TestObject(mock.Object, mock2.Object);

			var result = testObject.Function();
			Assert.AreEqual("foo", result);

			mock2.Setup(m => m.Function()).Returns("Blah");
			Assert.AreEqual("foo", result);

			testObject = new TestObject(mock.Object, mock2.Object);
			result = testObject.Function();
			Assert.AreEqual("foo", result);
		}

		[TestMethod]
		public void Moq_CallObjectPropertyAfterSetup_SetupAsExpected()
		{
			var target = CreateObjectOfType<TestObject>.WithMocks();

			target.MockOf<ISrvc2>().Setup(svc => svc.Function())
				.Returns("blah")
				.Verifiable();

			var result = target.MockOf<ISrvc2>().Object.Function();

			Assert.AreEqual("blah", result);
		}

		[TestMethod]
		public void Mock_SetupAndTargetVerify()
		{
			var target = CreateObjectOfType<TestObject>.WithMocks();

			target.MockOf<ISrvc1>()
				.Setup(svc => svc.MethodWithStringParameter(It.Is<string>(str => str == "foo")))
				.Verifiable();

			var testObject = target.Target;
			testObject.MethodWithStringParameter("foo");

			target.VerifyAll();

		}

		[TestMethod]
		public void Verify_ServiceMethodWithParameter_IsCalled()
		{
			var target = CreateObjectOfType<TestObject>.WithMocks();

			target.MockOf<ISrvc1>()
				.Setup(svc => svc.MethodWithStringParameter(It.IsAny<string>()))
				.Verifiable();

			var testObject = target.Target;
			testObject.MethodWithStringParameter("foo");

			target.Verify();
		}

		[TestMethod, ExpectedException(typeof(Moq.MockException))]
		public void Mock_MethodWithoutSetupOnStrict_ExpectedException()
		{
			var target = CreateObjectOfType<TestObject>.WithMocks(MockBehavior.Strict);

			target.MockOf<ISrvc1>()
				.Setup(svc => svc.MethodWithStringParameter(It.IsAny<string>()))
				.Verifiable();

			var testObject = target.Target;
			testObject.Method();
		}

		public interface ISrvc1 { void Method(); void MethodWithStringParameter(string value);}
		public interface ISrvc2 { string Function(); string FunctionWithStringParameter(string value);}

		public sealed class TestObject
		{
			ISrvc1 a;
			ISrvc2 b;

			public TestObject(ISrvc1 a, ISrvc2 b)
			{
				this.a = a;
				this.b = b;
			}

			public void Method() { a.Method(); }
			public void MethodWithStringParameter(string value) { a.MethodWithStringParameter(value); }

			public string Function() { b.Function(); return "foo"; }
			public string FunctionWithStringParameter(string value) { b.FunctionWithStringParameter(value); return value; }

		}
	}
}
