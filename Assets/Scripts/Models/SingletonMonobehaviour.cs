using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public abstract class SingletonMonobehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T s_instance;
    private static readonly object s_lock = new object();
    private static bool s_applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (s_applicationIsQuitting)
            {
                //Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed. Returning null.");
                return null;
            }

            lock (s_lock)
            {
                if (s_instance == null)
                {
                    s_instance = FindAnyObjectByType<T>();

                    if (s_instance == null)
                    {
                        GameObject singletonObject = new GameObject($"{typeof(T).Name}");
                        s_instance = singletonObject.AddComponent<T>();
                        DontDestroyOnLoad(singletonObject);
                    }
                }
                return s_instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (s_instance == null)
        {
            s_instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (s_instance != this)
        {
            Destroy(gameObject);
            return;
        }
        SingleAwake();

        SceneManager.activeSceneChanged -= SingleOnSceneChanged;
        SceneManager.activeSceneChanged += SingleOnSceneChanged;
    }

    protected virtual void OnEnable()
    {
        if (s_instance == null)
            s_instance = this as T;

        SingleOnEnable();
    }

    protected virtual void OnDestroy()
    {
        if (s_instance == this)
            s_instance = null;

        SingleOnDestroy();
    }

    protected virtual void OnApplicationQuit()
    {
        SingleOnApplicationQuit();
        s_applicationIsQuitting = true;
        s_instance = null;
        // Destroy(gameObject); // убрал, чтобы не ловить баги при выходе
    }

    // Методы для переопределения в наследниках
    protected virtual void SingleAwake() { }
    protected virtual void SingleOnEnable() { }
    protected virtual void SingleOnDestroy() { }
    protected virtual void SingleOnApplicationQuit() { }
    protected virtual void SingleOnSceneChanged(Scene s1, Scene s2) { }
}
