using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class LoadingCountDown : MonoBehaviour
{
    public struct LoadingTextDescriptor
    {
        public readonly string text;
        public readonly int lifeTime;

        public LoadingTextDescriptor(string text, int lifetime)
        {
            this.text = text;
            this.lifeTime = lifetime;
        }
    }

    public int firstTextTimeout = 10;
    public int secondTextTimeout = 10;
    public Text textInput1;
    public Text textInput2;
    public Text loadingText;
    public GameObject loadingParentObject;

    private List<LoadingTextDescriptor> loadingTexts;
    private string coroutine = "LoseTime";
    private int totalTime = 0;
    private int timeSpent = 0;

    void Start()
    {
        LoadingTextDescriptor a = new LoadingTextDescriptor(textInput1.text, firstTextTimeout);
        LoadingTextDescriptor b = new LoadingTextDescriptor(textInput2.text, secondTextTimeout);
        loadingTexts = new List<LoadingTextDescriptor>();
        loadingTexts.Add(a);
        loadingTexts.Add(b);

        loadingTexts.ForEach(delegate (LoadingTextDescriptor s) {
            totalTime += s.lifeTime;
        });

        StartCoroutine(coroutine);
        Time.timeScale = 1; 
    }
    void Update()
    {
        if (loadingTexts.Count == 0)
        {
            loadingParentObject.SetActive(false);
            StopCoroutine(coroutine);
        }
        else
        {
            LoadingTextDescriptor first = loadingTexts[0];
            if (first.lifeTime > timeSpent)
            {
                loadingText.text = first.text;
            }
            else
            {
                timeSpent -= first.lifeTime;
                totalTime -= first.lifeTime;
                loadingTexts.RemoveAt(0);
            }
        }
    }
    IEnumerator LoseTime()
    {
        while (!IsDue())
        {
            yield return new WaitForSeconds(1);
            timeSpent++;
        }
    }
    bool IsDue()
    {
        return (timeSpent >= totalTime);
    }
}