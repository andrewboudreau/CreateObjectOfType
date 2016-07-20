## CreateObjectOfType<>

CreateObjectOfType is a helper framework for reduce the amount of boilerplate code required to test objects using DI and Moq.

* Simplifies to the testing process of objects with changing or numerous constructor parameters.
* Designed to work in conjunction with Moq testing framework.
* Statisfies an objects constructor dependencies with Moq instances created on the fly.
* Supports both Strict and Loose behaviors.
* Keep your tests clean and understandable.


## USAGE EXAMPLE
```csharp
[TestMethod()]
public void Delete_PassInId_ReturnsRedirect()
{
	//Arrange
	var A = CreateObjectOfType<MyController>.WithMocks(MockBehavior.Strict);

	A.MockOf<IBackendService>()
		.Setup(r => r.DeleteSomething(It.IsAny<Guid>()))
		.Verifiable();

	//Act
	var controller = A.Target;
	var result = controller.Delete(Guid.NewGuid());

	//Assert
	A.Verify();
	Assert.IsTrue(result is RedirectToRouteResult);
}
```