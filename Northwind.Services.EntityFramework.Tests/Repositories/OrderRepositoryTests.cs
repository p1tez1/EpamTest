using System.Reflection;
using Northwind.Services.EntityFramework.Entities;
using Northwind.Services.EntityFramework.Repositories;
using NUnit.Framework;
using RepositoryOrder = Northwind.Services.Repositories.Order;

namespace Northwind.Services.EntityFramework.Tests.Repositories;

[TestFixture]
public sealed class OrderRepositoryTests : RepositoryTestsBase
{
    private static readonly object[][] ConstructorData =
    {
        new object[]
        {
            new[] { typeof(NorthwindContext) },
        },
    };

    [SetUp]
    public void SetUp()
    {
        this.ClassType = typeof(OrderRepository);
    }

    [Test]
    public void IsPublicClass()
    {
        this.AssertThatClassIsPublic(true);
    }

    [Test]
    public void InheritsObject()
    {
        this.AssertThatClassInheritsObject();
    }

    [TestCaseSource(nameof(ConstructorData))]
    public void HasPublicInstanceConstructor(Type[] parameterTypes)
    {
        this.AssertThatClassHasPublicConstructor(parameterTypes);
    }

    [Test]
    public void HasRequiredMembers()
    {
        Assert.AreEqual(0, this.ClassType.GetFields(BindingFlags.Instance | BindingFlags.Public).Length);
        Assert.AreEqual(0, this.ClassType.GetConstructors(BindingFlags.Static | BindingFlags.Public).Length);
        Assert.AreEqual(1, this.ClassType.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Length);
        Assert.AreEqual(0, this.ClassType.GetProperties(BindingFlags.Static | BindingFlags.Public).Length);
        Assert.AreEqual(0, this.ClassType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Length);
        Assert.AreEqual(0, this.ClassType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly).Length);
        Assert.AreEqual(5, this.ClassType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).Length);
        Assert.AreEqual(0, this.ClassType.GetEvents(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public).Length);
    }

    [TestCase("GetOrdersAsync", false, true, true, typeof(Task<IList<RepositoryOrder>>))]
    [TestCase("GetOrderAsync", false, true, true, typeof(Task<RepositoryOrder>))]
    [TestCase("AddOrderAsync", false, true, true, typeof(Task<long>))]
    [TestCase("RemoveOrderAsync", false, true, true, typeof(Task))]
    public void HasMethod(string methodName, bool isStatic, bool isPublic, bool isVirtual, Type returnType)
    {
        _ = this.AssertThatClassHasMethod(methodName, isStatic, isPublic, isVirtual, returnType);
    }
}
