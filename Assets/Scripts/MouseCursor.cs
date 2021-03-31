using UnityEngine;
using UnityEngine.UI;

public class MouseCursor : MonoBehaviour
{
    private Image renderer; 
    public Sprite pointer, defult;
    public static MouseCursor instance;
    void Start()
    {
        DontDestroyOnLoad(this);
        Cursor.visible = false;
        renderer = GetComponent<Image>();
    }
    void OnEnable()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }
    // Update is called once per frame
    void Update()
    {
        Vector2 pos = Input.mousePosition;//Camera.main.ScreenToWorldPoint(Input.mousePosition);
        gameObject.transform.position = new Vector2(pos.x, pos.y);
        if (Input.GetMouseButtonDown(0))
            renderer.sprite = pointer;
        else if (Input.GetMouseButtonUp(0))
            renderer.sprite = defult;
    }
}
