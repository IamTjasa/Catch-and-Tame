using System;

public static class QuestEvents
{
    public static event Action OnBallThrown;

    public static event Action<string> OnAnimalCaught;

    public static void BallThrown() => OnBallThrown?.Invoke();
    public static void AnimalCaught(string animalKind) => OnAnimalCaught?.Invoke(animalKind);
}
