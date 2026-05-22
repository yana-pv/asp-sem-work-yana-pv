using DeepMatch.Domain.Constants;

namespace DeepMatch.Domain.ValueObjects;

public class Rating
{
    public int Value { get; private set; }

    public Rating(int value = 0)
    {
        if (value < 0)
            throw new ArgumentException("Рейтинг не может быть отрицательным", nameof(value));
        Value = value;
    }

    public void Increase(int amount = BusinessRules.Rating.DefaultIncreaseAmount)
    {
        if (amount <= 0)
            throw new ArgumentException("Величина повышения рейтинга должна быть положительной", nameof(amount));
        Value += amount;
    }

    public int GetBonusSwipes()
    {
        return Value / BusinessRules.Rating.PointsPerBonusSwipe;
    }
}
