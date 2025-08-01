using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AnimalUIManager : MonoBehaviour
{
    public Transform panel;
    public List<Image> animalImages;

    private Dictionary<string, Image> imageLookup = new Dictionary<string, Image>();

    void Awake()
    {
        foreach (var img in animalImages)
        {
            if (img != null)
            {
                string key = img.name.Replace("Image", "").Trim();
                imageLookup[key] = img;
            }
        }
    }

    public void ReplaceAnimalImage(string animalName)
    {
        if (imageLookup.TryGetValue(animalName, out Image img))
        {
            Sprite newSprite = Resources.Load<Sprite>("AnimalImages/" + animalName);
            if (newSprite != null)
            {
                img.sprite = newSprite;
            }
        }
    }
}
