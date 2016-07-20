using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Moq;

namespace Ab.TestHelper
{
	/// <summary>
	/// Helper for instantiating an object of type <typeparamref name="TObject"/> with all of its 
	/// mocked constructor dependencies supplied.
	/// </summary>
	/// <typeparam name="TObject">Type of target object to create.</typeparam>
	public class CreateObjectOfType<TObject> : IHideObjectMembers
		where TObject : class
	{
		/// <summary>
		/// Instance of TObject.
		/// </summary>
		public TObject Target
		{
			get
			{
				var ctor = DefaultConstructorSelector<TObject>();

				// extract instances from Mock via Mock.Object property
				var parameters = new ArrayList();
				for (var i = 0; i < ParameterMocks.Count; i++)
				{
					var parameterInstance = typeof(Mock<>)
						.MakeGenericType(ObjectConstructorParameterTypes[i])
						.GetProperties()[0] // todo: fix this, there are two Object properties.
						.GetValue(ParameterMocks[i].Mock, null);
					parameters.Add(parameterInstance);
				}

				return (TObject)ctor.Invoke(parameters.ToArray());
			}
		}

		/// <summary>
		/// Use the static method BuildObjectWithDependencyMocks to construct instances.
		/// </summary>
		private CreateObjectOfType()
		{
			this.ParameterMocks = new List<IParameterMock>();
			this.ObjectConstructorParameterTypes = new List<Type>();
		}

		/// <summary>
		/// Builds an instance of type <typeparamref name="TTarget"/> with all dependencies mocked using <see cref="MockBehavior.Loose"/>.
		/// </summary>
		/// <typeparam name="TObject">Type of object created.</typeparam>
		/// <param name="typesToUseAlternateBehavior">These are the types you want to use the alternative mocking behavior.</param>
		/// <example>
		///		CreateObjectOfType&lt;MyObject&gt;.WithMocks(MockBehavior.Strict, typeof(IDependencyService), typeof(IDependencyService2));
		///		Would create an object of type MyObject where all constructor parameters are satisfied and mocked strict, EXCEPT for IDependencyService
		///		and IDependencyService2 will use MockBehavior.Loose.
		/// </example>
		/// <returns>A container object containing the target along with the auto-mocked dependencies.</returns>
		public static CreateObjectOfType<TObject> WithMocks(MockBehavior behavior, params Type[] typesToUseAlternateBehavior)
		{
			var container = new CreateObjectOfType<TObject>();
			var result = container.CreateConstructorParameterMocks<TObject>(behavior, typesToUseAlternateBehavior);
			container.ParameterMocks = result;

			return container;
		}

		/// <summary>
		/// Builds an instance of type <typeparamref name="TTarget"/> with all dependencies mocked using <see cref="MockBehavior.Loose"/>.
		/// </summary>
		/// <typeparam name="TObject">Type of object created.</typeparam>
		/// <returns>A container object containing the target along with the auto-mocked dependencies.</returns>
		public static CreateObjectOfType<TObject> WithMocks()
		{
			return WithMocks(MockBehavior.Loose, new Type[] { });
		}

		/// <summary>
		/// Builds an instance of type <typeparamref name="TTarget"/> with all dependencies mocked using <see cref="MockBehavior.Loose"/>.
		/// </summary>
		/// <typeparam name="TObject">Type of object created.</typeparam>
		/// <remarks>this is a syntax helper for param array overload, TDependency is use for alternative params array.</remarks>
		/// <returns>A container object containing the target along with the auto-mocked dependencies.</returns>
		public static CreateObjectOfType<TObject> WithMocks<TDependency>(MockBehavior behavior)
		{
			//get the 
			var objectWithMocks = WithMocks(behavior, typeof(TDependency));
			return objectWithMocks;
		}

		/// <summary>
		/// Builds an instance of type <typeparamref name="TTarget"/> with all dependencies mocked using <see cref="MockBehavior.Loose"/>.
		/// </summary>
		/// <typeparam name="TObject">Type of object created.</typeparam>
		/// <remarks>this is a syntax helper for param array overload</remarks>
		/// <returns>A container object containing the target along with the auto-mocked dependencies.</returns>
		public static CreateObjectOfType<TObject> WithMocks<TDependency, TDependency2>(MockBehavior behavior)
		{
			//get the 
			var objectWithMocks = WithMocks(behavior, typeof(TDependency), typeof(TDependency2));
			return objectWithMocks;
		}

		/// <summary>
		/// Returns the mock used to implement <typeparamref name="TParameter"/>.
		/// </summary>
		/// <typeparam name="TParameter">Type of parameter being mocked.</typeparam>
		/// <returns>A mock of <typeparamref name="TParameter"/>.</returns>
		public Mock<TParameter> MockOf<TParameter>()
			where TParameter : class
		{
			var mock = ParameterMocks.Single(p => p is ParameterMock<TParameter>);
			var typedMock = mock as ParameterMock<TParameter>;
			return typedMock.Value;
		}

		/// <summary>
		/// Verifies that all verifiable expectations have been met.
		/// </summary>
		public void Verify()
		{
			foreach (var parameter in ParameterMocks)
			{
				parameter.Mock.Verify();
			}
		}

		/// <summary>
		/// Verifies that all verifiable expectations have been met, even ones not marked as verifiable.
		/// </summary>
		public void VerifyAll()
		{
			foreach (var parameter in ParameterMocks)
			{
				parameter.Mock.VerifyAll();
			}
		}

		/// <summary>
		/// Verifies that Mock of <typeparamref name="TParameter"/> expectations have been met.
		/// </summary>
		/// <typeparam name="TParameter">Type of parameter being mocked.</typeparam>
		public void Verify<TTarget>()
				where TTarget : class
		{
			this.MockOf<TTarget>().Verify(); //this code won't compile?
			throw new NotImplementedException("MockedParameter<TObject>.Verify(); //this code won't compile?");
		}

		/// <summary>
		/// Verifies that Mock of <typeparamref name="TParameter"/> expectations have been met, even ones not marked as verifiable.
		/// </summary>
		/// <typeparam name="TParameter">Type of parameter being mocked.</typeparam>
		public void VerifyAll<TTarget>()
			where TTarget : class
		{
			this.MockOf<TTarget>().VerifyAll();
		}

		/// <summary>
		/// Creates all the mocks required to instantiate a new instance of type <typeparam name="TTarget">.
		/// </summary>
		/// <param name="behavior">The <see cref="MockBehavior"/> to use by default.</param>
		///	<param name="typeToUseAlternativeMockBehavior">Types which should use the alternative mock behavior to <paramref name="Behavior"/>.</param>
		/// <remarks>By default the TTarget's constructor containing the most parameters is used.</remarks>
		/// <example>
		///		Since there are only two options for MockBehavior, Strict and Loose. If you supply a default MockBehavior, any types supplied to typeToUseAlternativeMockBehavior will
		///		apply the alternative mocking behavior.
		///		
		///		All mocks are created using Loose behavior.
		///		CreateConstructorParameterMocksAndInstance(MockBehavior.Loose); 
		///		
		///		TService and TAnotherService mocks are created using MockBehavior.Strict, all other mocks use MockBehavior.Loose.
		///		CreateConstructorParameterMocksAndInstance(MockBehavior.Loose, typeof(TService), typeof(TAnotherService)); 
		/// </example>
		internal IList<IParameterMock> CreateConstructorParameterMocks<TTarget>(MockBehavior defaultBehavior, params Type[] typesToUseAlternateBehavior)
		{
			var ctor = DefaultConstructorSelector<TTarget>();
			var constructorParameters = ctor.GetParameters();
			var alternateBehavior = (defaultBehavior == MockBehavior.Loose) ? MockBehavior.Strict : MockBehavior.Loose;

			// construct collection of Mock<ParameterType>
			var mocks = from p in constructorParameters
									let mockConstructor = typeof(Mock<>).MakeGenericType(p.ParameterType).GetConstructor(new Type[] { typeof(MockBehavior) })
									let behavior = typesToUseAlternateBehavior.Contains(p.ParameterType) ? alternateBehavior : defaultBehavior
									select new
									{
										Mock = mockConstructor.Invoke(new object[] { behavior }),
										MockedType = p.ParameterType
									};

			ObjectConstructorParameterTypes = mocks.Select(m => m.MockedType).ToList();

			// construct collection ParameterMock<ParameterType>
			var parameterMocks = from mock in mocks
													 let genericParameterMock = typeof(Mock<>).MakeGenericType(mock.MockedType)
													 let parameterMockConstructor = typeof(ParameterMock<>).MakeGenericType(mock.MockedType).GetConstructor(new Type[] { genericParameterMock })
													 select parameterMockConstructor.Invoke(new object[] { mock.Mock }) as IParameterMock;

			return parameterMocks.ToList();
		}

		/// <summary>
		/// Selects the constructor method.
		/// </summary>
		/// <typeparam name="TTarget">Type being constructed.</typeparam>
		/// <returns>A constructor used to instantiate <typeparamref name="TTarget"/>.</returns>
		private static ConstructorInfo DefaultConstructorSelector<TTarget>()
		{
			ConstructorInfo ctor = default(ConstructorInfo);
			foreach (var c in typeof(TTarget).GetConstructors())
			{
				if (ctor == null || ctor.GetParameters().Count() <= c.GetParameters().Count())
				{
					ctor = c;
				}
			}
			return ctor;
		}

		private IList<IParameterMock> ParameterMocks { get; set; }
		private IList<Type> ObjectConstructorParameterTypes { get; set; }

	}
}
