// <copyright file="MapLabelDbTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Xunit;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using System;

namespace Pulse.Account.Infrastructure.Tests.Mappers
{
    public class MapLabelDbTests
    {
        [Fact]
        public void Map_Label_To_LabelEntity_Should_Return_Correct_Values()
        {
            // Arrange
            var label = new Label
            {
                Code = "TEST",
                CollaboratorLabel = "Test Collaborator",
                CustomerLabel = "Test Customer",
                Description = "Test Description",
                LabelId = 2,
                Business = "IT",
                IsVisible = true
            };

            // Act
            var result = MapLabelDb.Map(label);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(label.Code, result.Code);
            Assert.Equal(label.CollaboratorLabel, result.CollaboratorLabel);
            Assert.Equal(label.CustomerLabel, result.CustomerLabel);
            Assert.Equal(label.Description, result.Description);
            Assert.Equal(label.LabelId, result.LabelId);
        }

        [Fact]
        public void Map_Label_To_LabelEntity_With_Null_Description_Should_Map_Correctly()
        {
            // Arrange
            var label = new Label
            {
                Code = "TEST",
                CollaboratorLabel = "Test Collaborator",
                CustomerLabel = "Test Customer",
                Description = null,
                LabelId = 2,
                Business = "IT",
                IsVisible = true
            };

            // Act
            var result = MapLabelDb.Map(label);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(label.Code, result.Code);
            Assert.Equal(label.CollaboratorLabel, result.CollaboratorLabel);
            Assert.Equal(label.CustomerLabel, result.CustomerLabel);
            Assert.Null(result.Description);
            Assert.Equal(label.LabelId, result.LabelId);
        }

        [Fact]
        public void Map_Null_Label_To_LabelEntity_Should_Return_Null()
        {
            // Arrange
            Label? label = null;

            // Act
            var result = MapLabelDb.Map(label);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Map_LabelEntity_To_Label_Should_Return_Correct_Values()
        {
            // Arrange
            var labelEntity = new LabelEntity
            {
                Code = "TEST",
                CollaboratorLabel = "Test Collaborator",
                CustomerLabel = "Test Customer",
                Description = "Test Description",
                LabelId = 2,
                Business = "IT",
                IsVisible = true
            };

            // Act
            var result = MapLabelDb.Map(labelEntity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(labelEntity.Code, result.Code);
            Assert.Equal(labelEntity.CollaboratorLabel, result.CollaboratorLabel);
            Assert.Equal(labelEntity.CustomerLabel, result.CustomerLabel);
            Assert.Equal(labelEntity.Description, result.Description);
            Assert.Equal(labelEntity.LabelId, result.LabelId);
        }

        [Fact]
        public void Map_Null_LabelEntity_To_Label_Should_Return_Null()
        {
            // Arrange
            LabelEntity? labelEntity = null;

            // Act
            var result = MapLabelDb.Map(labelEntity);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Map_RoleLabel_To_RoleLabelEntity_Should_Return_Correct_Values()
        {
            // Arrange
            var roleLabel = new RoleLabel
            {
                AccountId = 12345,
                ContactId = 67890,
                CreatedBy = 101,
                CreatedDate = DateTime.UtcNow,
                LabelId = 202
            };

            // Act
            var result = MapLabelDb.Map(roleLabel);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(roleLabel.AccountId, result.AccountId);
            Assert.Equal(roleLabel.ContactId, result.ContactId);
            Assert.Equal(roleLabel.CreatedBy, result.CreatedBy);
            Assert.Equal(roleLabel.CreatedDate, result.CreatedDate);
            Assert.Equal(roleLabel.LabelId, result.LabelId);
        }

        [Fact]
        public void Map_Null_RoleLabel_To_RoleLabelEntity_Should_Return_Null()
        {
            // Arrange
            RoleLabel? roleLabel = null;

            // Act
            var result = MapLabelDb.Map(roleLabel);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Map_RoleLabelEntity_To_RoleLabel_Should_Return_Correct_Values()
        {
            // Arrange
            var roleLabelEntity = new RoleLabelEntity
            {
                AccountId = 12345,
                ContactId = 67890,
                CreatedBy = 101,
                CreatedDate = DateTime.UtcNow,
                LabelId = 202
            };

            // Act
            var result = MapLabelDb.Map(roleLabelEntity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(roleLabelEntity.AccountId, result.AccountId);
            Assert.Equal(roleLabelEntity.ContactId, result.ContactId);
            Assert.Equal(roleLabelEntity.CreatedBy, result.CreatedBy);
            Assert.Equal(roleLabelEntity.CreatedDate, result.CreatedDate);
            Assert.Equal(roleLabelEntity.LabelId, result.LabelId);
        }

        [Fact]
        public void Map_Null_RoleLabelEntity_To_RoleLabel_Should_Return_Null()
        {
            // Arrange
            RoleLabelEntity? roleLabelEntity = null;

            // Act
            var result = MapLabelDb.Map(roleLabelEntity);

            // Assert
            Assert.Null(result);
        }
    }
}
