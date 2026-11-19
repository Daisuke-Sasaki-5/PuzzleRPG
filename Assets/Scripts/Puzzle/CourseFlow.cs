using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class CourseFlow
{
    public static bool IsLoading
    {
        get;
        private set;
    }
    private static AsyncOperation pending;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        IsLoading = false;
        pending = null;
    }
    public static void MyPage()
    {
        Load("MyPage");
    }
    public static void Battle()
    {
        Load("Battle");
    }
    public static void Result()
    {
        Load("Result");
    }
    private static void Load(string name)
    {
        if (IsLoading)
        {
            return;
        }
        IsLoading = true;
        try
        {
            var op = SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);
            if (op == null)
            {
                throw new
                    InvalidOperationException("Scene not registered: " +
                    name);
            }
            pending = op;
            op.completed += _ =>
            {
                if (pending == op)
                {
                    pending = null;
                    IsLoading = false;
                }
            };
        }
        catch
        {
            pending = null;
            IsLoading = false;
            throw;
        }
    }
}