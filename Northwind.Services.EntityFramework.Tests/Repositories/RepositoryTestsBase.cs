using System.Reflection;
using NUnit.Framework;

namespace Northwind.Services.EntityFramework.Tests.Repositories;

public abstract class RepositoryTestsBase
{
    protected Type ClassType { get; set; } = default!;

    protected void AssertThatClassIsPublic(bool isSealed)
    {
        Assert.That(this.ClassType.IsClass, Is.True);
        Assert.That(this.ClassType.IsPublic, Is.True);
        Assert.That(this.ClassType.IsAbstract, Is.False);
        Assert.That(this.ClassType.IsSealed, isSealed ? Is.True : Is.False);
    }

    protected void AssertThatClassInheritsObject()
    {
        Assert.That(this.ClassType.BaseType, Is.EqualTo(typeof(object)));
    }

    protected void AssertThatClassHasPublicConstructor(Type[] parameterTypes)
    {
        var constructorInfo = this.ClassType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, parameterTypes, null);
        Assert.That(constructorInfo, Is.Not.Null);
    }

    protected MethodInfo? AssertThatClassHasMethod(string methodName, bool isStatic, bool isPublic, bool isVirtual, Type returnType)
    {
        var methodInfo = this.ClassType!.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(methodInfo!.IsStatic, isStatic ? Is.True : Is.False);
        Assert.That(methodInfo!.IsPublic, isPublic ? Is.True : Is.False);
        Assert.That(methodInfo!.IsVirtual, isVirtual ? Is.True : Is.False);
        Assert.That(methodInfo!.ReturnType, Is.EqualTo(returnType));

        return methodInfo;
    }
}
