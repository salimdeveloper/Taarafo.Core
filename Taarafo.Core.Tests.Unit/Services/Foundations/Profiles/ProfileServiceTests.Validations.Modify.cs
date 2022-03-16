﻿// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

using System.Threading.Tasks;
using Moq;
using Taarafo.Core.Models.Profiles;
using Taarafo.Core.Models.Profiles.Exceptions;
using Xunit;

namespace Taarafo.Core.Tests.Unit.Services.Foundations.Profiles
{
    public partial class ProfileServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfProfileIsNullAndLogItAsync()
        {
            // given
            Profile nullProfile = null;
            var nullProfileException = new NullProfileException();

            var expectedProfileValidationException =
                new ProfileValidationException(nullProfileException);

            // when
            ValueTask<Profile> modifyProfileTask =
                this.profileService.ModifyProfileAsync(nullProfile);

            // then
            await Assert.ThrowsAsync<ProfileValidationException>(() =>
                modifyProfileTask.AsTask());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedProfileValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateProfileAsync(It.IsAny<Profile>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnModifyIfProfileIsInvalidAndLogItAsync(string invalidText)
        {
            // given 
            var invalidProfile = new Profile
            {
                Username = invalidText
            };

            var invalidProfileException = new InvalidProfileException();

            invalidProfileException.AddData(
                key: nameof(Profile.Id),
                values: "Id is required");

            invalidProfileException.AddData(
                key: nameof(Profile.Name),
                values: "Text is required");

            invalidProfileException.AddData(
                key: nameof(Profile.Username),
                values: "Text is required");

            invalidProfileException.AddData(
                key: nameof(Profile.Email),
                values: "Text is required");

            invalidProfileException.AddData(
                key: nameof(Profile.CreatedDate),
                values: "Date is required");

            invalidProfileException.AddData(
                key: nameof(Profile.UpdatedDate),
                "Date is required",
                $"Date is the same as {nameof(Profile.CreatedDate)}");

            var expectedProfileValidationException =
                new ProfileValidationException(invalidProfileException);

            // when
            ValueTask<Profile> modifyProfileTask =
                this.profileService.ModifyProfileAsync(invalidProfile);

            //then
            await Assert.ThrowsAsync<ProfileValidationException>(() =>
                modifyProfileTask.AsTask());

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedProfileValidationException))),
                        Times.Once());

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateProfileAsync(It.IsAny<Profile>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
