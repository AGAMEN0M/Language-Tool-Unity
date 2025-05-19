using UnityEditor;
using UnityEngine;

public class MovableBorderEditorWindow : EditorWindow
{
    private Rect boxRect = new(50, 50, 200, 200); // Posição e tamanho inicial.
    private Vector2 offset; // Deslocamento para mover o quadrado.
    private const float borderThickness = 10f; // Espessura da borda interativa.
    private bool isDragging = false; // Flag para controle de arrasto.
    private Color borderColor = Color.black;

    [MenuItem("Window/Border Box/Movable Border")]
    public static void ShowWindow()
    {
        GetWindow<MovableBorderEditorWindow>("Movable Border");
    }

    private void OnGUI()
    {
        Event e = Event.current;

        // Desenha o quadrado vermelho.
        EditorGUI.DrawRect(boxRect, Color.red);

        // Desenha a borda ao redor do quadrado.
        DrawBorder(boxRect);

        // Detecta a posição do mouse em relação à borda e altera a cor.
        UpdateBorderColor(e.mousePosition);

        // Detecta interação do mouse.
        HandleMouseInput(e);

        // Atualiza a tela.
        Repaint();
    }

    private void HandleMouseInput(Event e)
    {
        if (e.type == EventType.MouseDown)
        {
            if (IsMouseOnBorder(e.mousePosition))
            {
                // Inicia o arrasto.
                isDragging = true;
                offset = e.mousePosition - new Vector2(boxRect.x, boxRect.y);
                e.Use(); // Usar o evento para evitar propagação.
            }
        }

        // Detecta o movimento do mouse (arrasto).
        if (e.type == EventType.MouseDrag && isDragging)
        {
            // Arrasta o quadrado.
            boxRect.x = e.mousePosition.x - offset.x;
            boxRect.y = e.mousePosition.y - offset.y;
            e.Use();
        }

        // Finaliza o arrasto.
        if (e.type == EventType.MouseUp)
        {
            isDragging = false;
        }
    }

    private void UpdateBorderColor(Vector2 mousePosition)
    {
        if (IsMouseOnBorder(mousePosition))
        {
            borderColor = Color.white;
        }
        else
        {
            borderColor = Color.black;
        }
    }

    private void DrawBorder(Rect rect)
    {
        float borderInset = borderThickness / 2;

        // Desenha as 4 bordas usando um método auxiliar.
        DrawSideBorder(rect.x - borderInset, rect.y - borderInset, rect.width + borderThickness, borderThickness, borderColor);
        DrawSideBorder(rect.x - borderInset, rect.y + rect.height - borderInset, rect.width + borderThickness, borderThickness, borderColor);
        DrawSideBorder(rect.x - borderInset, rect.y - borderInset, borderThickness, rect.height + borderThickness, borderColor);
        DrawSideBorder(rect.x + rect.width - borderInset, rect.y - borderInset, borderThickness, rect.height + borderThickness, borderColor);
    }

    private void DrawSideBorder(float x, float y, float width, float height, Color color)
    {
        EditorGUI.DrawRect(new Rect(x, y, width, height), color);
    }

    private bool IsMouseOnBorder(Vector2 mousePos)
    {
        float borderInset = borderThickness;

        bool isOnTopBorder = mousePos.y >= boxRect.y - borderInset && mousePos.y <= boxRect.y;
        bool isOnBottomBorder = mousePos.y >= boxRect.y + boxRect.height && mousePos.y <= boxRect.y + boxRect.height + borderInset;
        bool isOnLeftBorder = mousePos.x >= boxRect.x - borderInset && mousePos.x <= boxRect.x;
        bool isOnRightBorder = mousePos.x >= boxRect.x + boxRect.width && mousePos.x <= boxRect.x + boxRect.width + borderInset;

        return isOnTopBorder || isOnBottomBorder || isOnLeftBorder || isOnRightBorder;
    }
}