using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class SceneTouchSwitch : MonoBehaviour
{
    [Header("Target (leave empty to auto-toggle)")]
    public string targetScene = "";
    public string[] toggleScenes = { "Main", "Playground" };

    [Header("Touch filter")]
    public LayerMask allowedLayers = ~0;   
    public string requiredTag = "";        
    public float holdTime = 0.20f;
    public float startDelay = 1.0f;
    public float cooldown = 1.0f;

    [Header("Lifecycle")]
    public bool persistent = false;

    [Header("Debug")]
    public bool debugLog = false;

    float sceneStartTime, lockUntil, timer;
    readonly HashSet<Collider> touching = new();

    Collider col;
    Rigidbody rb;

    void Reset()
    {
        var c = GetComponent<Collider>();
        if (c) c.isTrigger = true;
    }

    void Awake()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true;

        rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        if (persistent)
        {
            var others = FindObjectsOfType<SceneTouchSwitch>(true).Where(o => o != this && o.persistent);
            if (others.Any()) { Destroy(gameObject); return; }
            DontDestroyOnLoad(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        sceneStartTime = Time.time;
        timer = 0f;
        lockUntil = 0f;
        touching.Clear();
    }

    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        sceneStartTime = Time.time;
        timer = 0f;
        lockUntil = 0f;
        touching.Clear();
        if (debugLog) Debug.Log($"[SceneTouchSwitch] Loaded {s.name}");
    }

    void Update()
    {
        string target = GetTargetSceneName();
        if (string.IsNullOrEmpty(target)) return;
        if (!Application.CanStreamedLevelBeLoaded(target)) { if (debugLog) Debug.LogError($"Scene '{target}' not in Build Settings"); return; }
        if (SceneManager.GetActiveScene().name == target) return;
        if (Time.time - sceneStartTime < startDelay || Time.time < lockUntil) { timer = 0f; return; }

        bool isTouching = touching.Count > 0;
        timer = isTouching ? timer + Time.deltaTime : 0f;

        if (isTouching && timer >= holdTime)
        {
            if (debugLog) Debug.Log($"[SceneTouchSwitch] Touch  load {target}");
            lockUntil = Time.time + cooldown;
            SceneManager.LoadScene(target, LoadSceneMode.Single);
        }
    }

    string GetTargetSceneName()
    {
        if (!string.IsNullOrEmpty(targetScene)) return targetScene;
        var current = SceneManager.GetActiveScene().name;
        if (toggleScenes == null || toggleScenes.Length == 0) return null;
        int idx = System.Array.IndexOf(toggleScenes, current);
        return idx >= 0 ? toggleScenes[(idx + 1) % toggleScenes.Length] : toggleScenes[0];
    }

    bool Accept(Collider other)
    {
        if (!other) return false;
        if (allowedLayers != (allowedLayers | (1 << other.gameObject.layer))) return false;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return false;
        return true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (Accept(other))
        {
            touching.Add(other);
            if (debugLog) Debug.Log($"[SceneTouchSwitch] Enter: {other.name}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (touching.Remove(other) && debugLog)
            Debug.Log($"[SceneTouchSwitch] Exit: {other.name}");
    }

   
    void OnCollisionEnter(Collision c) { if (!col.isTrigger && Accept(c.collider)) touching.Add(c.collider); }
    void OnCollisionExit(Collision c) { if (!col.isTrigger) touching.Remove(c.collider); }
}
