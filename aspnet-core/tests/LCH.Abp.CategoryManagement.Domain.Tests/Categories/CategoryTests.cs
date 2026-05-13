using System;
using LCH.Abp.CategoryManagement.Categories;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace LCH.Abp.CategoryManagement.Tests.Categories;

public class CategoryTests
{
    [Fact]
    public void Create_Should_Set_Properties_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "animation";
        var displayName = "动画";
        var parentId = null as Guid?;
        var icon = "icons/animation.png";
        var description = "动画分区";
        var sortOrder = 1;

        // Act
        var category = Category.Create(id, name, displayName, parentId, icon, description, sortOrder);

        // Assert
        category.Id.ShouldBe(id);
        category.Name.ShouldBe(name);
        category.DisplayName.ShouldBe(displayName);
        category.ParentId.ShouldBe(parentId);
        category.Icon.ShouldBe(icon);
        category.Description.ShouldBe(description);
        category.SortOrder.ShouldBe(sortOrder);
        category.Level.ShouldBe(0); // 主分区 Level = 0
        category.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void Create_With_ParentId_Should_Set_Level_To_1()
    {
        // Arrange
        var id = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var name = "bangumi";
        var displayName = "番剧";

        // Act
        var category = Category.Create(id, name, displayName, parentId);

        // Assert
        category.ParentId.ShouldBe(parentId);
        category.Level.ShouldBe(1); // 子分区 Level = 1
    }

    [Fact]
    public void Create_Should_Throw_When_Name_Is_Null()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Category.Create(id, null!, "显示名称"));
    }

    [Fact]
    public void Create_Should_Throw_When_Name_Is_Empty()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Category.Create(id, "", "显示名称"));
    }

    [Fact]
    public void Create_Should_Throw_When_DisplayName_Is_Null()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Category.Create(id, "name", null!));
    }

    [Fact]
    public void Update_Should_Set_Properties_Correctly()
    {
        // Arrange
        var category = Category.Create(Guid.NewGuid(), "animation", "动画");
        var newName = "game";
        var newDisplayName = "游戏";
        var newIcon = "icons/game.png";
        var newDescription = "游戏分区";

        // Act
        category.Update(newName, newDisplayName, newIcon, newDescription);

        // Assert
        category.Name.ShouldBe(newName);
        category.DisplayName.ShouldBe(newDisplayName);
        category.Icon.ShouldBe(newIcon);
        category.Description.ShouldBe(newDescription);
    }

    [Fact]
    public void Enable_Should_Set_IsEnabled_To_True()
    {
        // Arrange
        var category = Category.Create(Guid.NewGuid(), "animation", "动画");
        category.Disable();

        // Act
        category.Enable();

        // Assert
        category.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void Disable_Should_Set_IsEnabled_To_False()
    {
        // Arrange
        var category = Category.Create(Guid.NewGuid(), "animation", "动画");

        // Act
        category.Disable();

        // Assert
        category.IsEnabled.ShouldBeFalse();
    }

    [Fact]
    public void SetSortOrder_Should_Update_SortOrder()
    {
        // Arrange
        var category = Category.Create(Guid.NewGuid(), "animation", "动画", sortOrder: 1);

        // Act
        category.SetSortOrder(5);

        // Assert
        category.SortOrder.ShouldBe(5);
    }
}