using UnityEditor;
using UnityEngine;

public class ResizableBorderEditorWindow : EditorWindow
{
    private Rect boxRect = new(50, 50, 200, 200); // Posição e tamanho inicial.
    private const float borderThickness = 10f; // Espessura da borda interativa.
    private bool isResizing = false; // Flag para controle de redimensionamento.
    private Vector2 resizeStartPos; // Posição inicial do mouse para redimensionamento.
    private Vector2 originalSize; // Tamanho original do quadrado.
    private Vector2 originalPosition; // Posição original do quadrado.
    private Color borderColor = Color.black;

    [MenuItem("Window/Border Box/Resizable Border")]
    public static void ShowWindow()
    {
        GetWindow<ResizableBorderEditorWindow>("Resizable Border");
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
                // Inicia o redimensionamento.
                isResizing = true;
                resizeStartPos = e.mousePosition;
                originalSize = boxRect.size;
                originalPosition = boxRect.position;
                e.Use(); // Usar o evento para evitar propagação.
            }
        }

        // Detecta o movimento do mouse (redimensionamento).
        if (e.type == EventType.MouseDrag && isResizing)
        {
            Vector2 mouseDelta = e.mousePosition - resizeStartPos;

            // Redimensiona o quadrado dependendo da borda que está sendo arrastada.
            if (IsMouseOnRightBorder(e.mousePosition)) // Lado direito
            {
                // Se o mouse se move para a direita, aumenta o tamanho.
                boxRect.width = Mathf.Max(originalSize.x + mouseDelta.x, 10f); // Evitar tamanho negativo
            }
            else if (IsMouseOnLeftBorder(e.mousePosition)) // Lado esquerdo
            {
                // Se o mouse se move para a esquerda, diminui o tamanho e move a posição.
                boxRect.x = originalPosition.x + mouseDelta.x;
                boxRect.width = Mathf.Max(originalSize.x - mouseDelta.x, 10f); // Evitar tamanho negativo
            }

            if (IsMouseOnBottomBorder(e.mousePosition)) // Lado inferior
            {
                // Se o mouse se move para baixo, aumenta o tamanho.
                boxRect.height = Mathf.Max(originalSize.y + mouseDelta.y, 10f); // Evitar tamanho negativo
            }
            else if (IsMouseOnTopBorder(e.mousePosition)) // Lado superior
            {
                // Se o mouse se move para cima, diminui o tamanho e move a posição.
                boxRect.y = originalPosition.y + mouseDelta.y;
                boxRect.height = Mathf.Max(originalSize.y - mouseDelta.y, 10f); // Evitar tamanho negativo
            }

            e.Use();
        }

        // Finaliza o redimensionamento.
        if (e.type == EventType.MouseUp)
        {
            isResizing = false;
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

    private bool IsMouseOnTopBorder(Vector2 mousePos)
    {
        return mousePos.y >= boxRect.y - borderThickness && mousePos.y <= boxRect.y;
    }

    private bool IsMouseOnBottomBorder(Vector2 mousePos)
    {
        return mousePos.y >= boxRect.y + boxRect.height && mousePos.y <= boxRect.y + boxRect.height + borderThickness;
    }

    private bool IsMouseOnLeftBorder(Vector2 mousePos)
    {
        return mousePos.x >= boxRect.x - borderThickness && mousePos.x <= boxRect.x;
    }

    private bool IsMouseOnRightBorder(Vector2 mousePos)
    {
        return mousePos.x >= boxRect.x + boxRect.width && mousePos.x <= boxRect.x + boxRect.width + borderThickness;
    }
}