using System;
using UnityEngine;
using TMPro;

public enum QuestType
{
    CatchAnyAnimal,        
    CatchSpecificAnimal,  
    ThrowBalls           
}

[Serializable]
public class Quest
{
    public QuestType type;
    public string targetAnimal;   
    public int goal;
    public int current;

    public string Description =>
        type switch
        {
            QuestType.CatchAnyAnimal => $"Catch {goal} animal{(goal > 1 ? "s" : "")}",
            QuestType.CatchSpecificAnimal => $"Catch {goal} {targetAnimal}{(goal > 1 ? "s" : "")}",
            QuestType.ThrowBalls => $"Throw {goal} ball{(goal > 1 ? "s" : "")}",
            _ => "Quest"
        };

    public bool IsCompleted => current >= goal;
}

public class QuestManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI questText;

    [Header("Nastavitve")]
    [SerializeField] private string[] animalPool = { "Cat", "Dog", "Dragon" };
    [SerializeField] private Vector2Int throwBallsRange = new Vector2Int(5, 8); 
    [SerializeField] private float newQuestDelay = 2f; 
    [SerializeField] private bool showCompletedBanner = true;

    private Quest active;

    void OnEnable()
    {
        QuestEvents.OnBallThrown += HandleBallThrown;
        QuestEvents.OnAnimalCaught += HandleAnimalCaught;
    }

    void OnDisable()
    {
        QuestEvents.OnBallThrown -= HandleBallThrown;
        QuestEvents.OnAnimalCaught -= HandleAnimalCaught;
    }

    void Start()
    {
        GenerateNewQuest();
    }

    void HandleBallThrown()
    {
        if (active == null || active.type != QuestType.ThrowBalls) return;
        active.current++;
        UpdateUI();
        CheckComplete();
    }

    void HandleAnimalCaught(string animalKind)
    {
        if (active == null) return;

        if (active.type == QuestType.CatchAnyAnimal)
        {
            active.current++;
        }
        else if (active.type == QuestType.CatchSpecificAnimal)
        {
            if (string.Equals(active.targetAnimal, animalKind, StringComparison.OrdinalIgnoreCase))
                active.current++;
        }
        else
        {
            return;
        }

        UpdateUI();
        CheckComplete();
    }

    void CheckComplete()
    {
        if (!active.IsCompleted) return;

        if (showCompletedBanner && questText != null)
            questText.text = active.Description + "  (Completed!)";

        Invoke(nameof(GenerateNewQuest), newQuestDelay);
    }

    void UpdateUI()
    {
        if (questText == null) return;
        questText.text = $"{active.Description}  ({Mathf.Min(active.current, active.goal)}/{active.goal})";
    }

    void GenerateNewQuest()
    {
        active = CreateRandomQuest();
        UpdateUI();
    }

    Quest CreateRandomQuest()
    {
        
        int roll = UnityEngine.Random.Range(0, 3);

        var q = new Quest { current = 0 };

        switch (roll)
        {
            case 0: 
                q.type = QuestType.CatchAnyAnimal;
                q.goal = 1; 
                break;

            case 1: 
                q.type = QuestType.CatchSpecificAnimal;
                q.goal = 1;
                q.targetAnimal = animalPool[UnityEngine.Random.Range(0, animalPool.Length)];
                break;

            default: 
                q.type = QuestType.ThrowBalls;
                q.goal = UnityEngine.Random.Range(throwBallsRange.x, throwBallsRange.y + 1);
                break;
        }

        return q;
    }
}
