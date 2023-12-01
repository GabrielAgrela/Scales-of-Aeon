using System;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    public Texture2D cursorArrow;

    private Texture2D currentPointer;

    public Texture2D clickCursor;

    [SerializeField]
    private GameObject _cardUI;
    [SerializeField]
    private GameObject _gameOverUI;

    [SerializeField]
    private AudioClip _clickSound;
    private AudioSource audioSource;
    void Start()
    {
        currentPointer = cursorArrow;
        Cursor.SetCursor(cursorArrow, Vector2.zero, CursorMode.ForceSoftware);
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.clip = _clickSound;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        CursorVisibility();
    }

    public void PlayClickSound()
    {
        audioSource.Play();
    }
    public void ResetPointer()
    {
        Cursor.SetCursor(currentPointer, Vector2.zero, CursorMode.ForceSoftware);
    }

    public void SetClickPointer()
    {
        // Cursor.SetCursor(clickCursor, Vector2.zero, CursorMode.ForceSoftware);
    }

    public void CursorVisibility()
    {
        SetVisibility(_gameOverUI.activeSelf || _cardUI.activeSelf);
    }

    public void SetVisibility(bool visible)
    {
        Cursor.visible = visible;
    }

    public bool isMouseActive()
    {
        return Cursor.visible;
    }
}
