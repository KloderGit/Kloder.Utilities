using Utilities;

namespace Tests;

public class UnitTest1
{
    public class PhoneTests
    {
        // Russian phone number pattern: +7 followed by 10 digits
        [Fact]
        public void Constructor_ShouldSetCorrectly_WhenValidRussianPhoneNumberGiven()
        {
            string validRussianPhoneNumber = "+71234567890";
            Phone phone = new Phone(validRussianPhoneNumber);

            Assert.Equal(validRussianPhoneNumber, phone.ToString());
        }

        [Theory]
        [InlineData("+71234567890", "+71234567890")]
        [InlineData("+71234567890", "81234567890")]
        public void Equals_ShouldReturnTrue_WhenEqualRussianPhonesGiven(string phoneNumber1, string phoneNumber2)
        {
            Phone phone1 = new Phone(phoneNumber1);
            Phone phone2 = new Phone(phoneNumber2);

            Assert.True(phone1.Equals(phone2));
            Assert.True(phone1.Equals(phoneNumber2));
        }

        [Theory]
        [InlineData("+71234567890", "+79876543210")]
        [InlineData("+71234567890", "89876543210")]
        [InlineData("+71234567890", "9876543210")]
        public void Equals_ShouldReturnFalse_WhenDifferentRussianPhonesGiven(string phoneNumber1, string phoneNumber2)
        {
            Phone phone1 = new Phone(phoneNumber1);
            Phone phone2 = new Phone(phoneNumber2);

            Assert.False(phone1.Equals(phone2));
            Assert.False(phone1.Equals(phoneNumber2));
        }

        [Theory]
        [InlineData("+71234567890")]
        [InlineData("81234567890")]
        [InlineData("1234567890")]
        public void TryParse_ShouldReturnTrue_WhenValidRussianPhoneNumberGiven(string phoneNumber)
        {
            Assert.True(Phone.TryParse(phoneNumber, out Phone result));
        }

        [Theory]
        [InlineData("+71234")]
        [InlineData("8123arcg")]
        [InlineData("abc1234567890")]
        public void TryParse_ShouldReturnFalse_WhenInvalidPhoneNumberGiven(string phoneNumber)
        {
            Assert.False(Phone.TryParse(phoneNumber, out Phone result));
        }

        [Theory]
        [InlineData("+71234567890")]
        [InlineData("81234567890")]
        [InlineData("1234567890")]
        public void OperatorOverloads_ShouldWorkCorrectly(string phoneNumber)
        {
            Phone phone = (Phone)phoneNumber;
            string phoneNumberFromPhone = (string)phone;

            Assert.Equal(phone, phoneNumberFromPhone);
        }
    }
}