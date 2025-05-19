using UnityEngine;
using UnityEditor;

public class DraggableAndResizableDorderEditorWindow : EditorWindow
{
    private Rect boxRect = new(50, 50, 200, 200); // Posição e tamanho inicial.
    private Vector2 offset; // Deslocamento para mover o quadrado.
    private const float borderThickness = 10f; // Espessura da borda interativa.
    private const float handleSize = 10f; // Tamanho dos quadrados verdes para redimensionamento.
    private bool isDragging = false; // Flag para controle de arrasto.
    private bool isResizing = false; // Flag para controle de redimensionamento.
    private Vector2 resizePivot; // O ponto de referência do redimensionamento.

    [MenuItem("Window/Border Box/Draggable and Resizable Border")]
    public static void ShowWindow()
    {
        GetWindow<DraggableAndResizableDorderEditorWindow>("Draggable and Resizable Border");
    }

    private void OnGUI()
    {
        Event e = Event.current;

        // Desenha o quadrado vermelho.
        EditorGUI.DrawRect(boxRect, Color.red);

        // Desenha a borda ao redor do quadrado.
        DrawBorder(boxRect);

        // Desenha os quadrados verdes nas 4 esquinas.
        DrawResizeHandles(boxRect);

        // Lidar com o evento de clique do mouse.
        HandleMouseEvents(e);

        // Detectar arrasto ou redimensionamento.
        HandleMouseDrag(e);

        // Finalizar o evento de soltar o mouse.
        if (e.type == EventType.MouseUp)
        {
            isDragging = false;
            isResizing = false;
        }
    }

    private void HandleMouseEvents(Event e)
    {
        // Verifica se o mouse está sobre um quadrado de redimensionamento.
        if (e.type == EventType.MouseDown && IsMouseOnResizeHandle(e.mousePosition))
        {
            isResizing = true;
            resizePivot = GetResizePivot(e.mousePosition); // Determina o ponto de referência.
            e.Use(); // Usar o evento para evitar propagação.
        }
        // Verifica se o mouse está dentro da área da borda para arrastar.
        else if (e.type == EventType.MouseDown && !isResizing && IsMouseOnBorder(e.mousePosition))
        {
            isDragging = true;
            offset = e.mousePosition - new Vector2(boxRect.x, boxRect.y);
            e.Use(); // Usar o evento para evitar propagação.
        }
    }

    private void HandleMouseDrag(Event e)
    {
        if (e.type == EventType.MouseDrag)
        {
            if (isDragging && !isResizing)
            {
                // Arrasta o quadrado.
                boxRect.x = e.mousePosition.x - offset.x;
                boxRect.y = e.mousePosition.y - offset.y;
                Repaint();
            }
            else if (isResizing)
            {
                // Lógica de redimensionamento.
                Vector2 mouseDelta = e.mousePosition - resizePivot;

                if (resizePivot == new Vector2(boxRect.x, boxRect.y)) // Canto superior esquerdo.
                {
                    boxRect.width -= mouseDelta.x;
                    boxRect.height -= mouseDelta.y;
                    boxRect.x += mouseDelta.x;
                    boxRect.y += mouseDelta.y;
                }
                else if (resizePivot == new Vector2(boxRect.x + boxRect.width, boxRect.y)) // Canto superior direito.
                {
                    boxRect.width += mouseDelta.x;
                    boxRect.height -= mouseDelta.y;
                    boxRect.y += mouseDelta.y;
                }
                else if (resizePivot == new Vector2(boxRect.x, boxRect.y + boxRect.height)) // Canto inferior esquerdo.
                {
                    boxRect.width -= mouseDelta.x;
                    boxRect.height += mouseDelta.y;
                    boxRect.x += mouseDelta.x;
                }
                else if (resizePivot == new Vector2(boxRect.x + boxRect.width, boxRect.y + boxRect.height)) // Canto inferior direito.
                {
                    boxRect.width += mouseDelta.x;
                    boxRect.height += mouseDelta.y;
                }

                // Garante um tamanho mínimo.
                boxRect.width = Mathf.Max(boxRect.width, 50f);
                boxRect.height = Mathf.Max(boxRect.height, 50f);

                // Atualiza o pivô para o novo valor.
                resizePivot = e.mousePosition;
                Repaint();
            }
        }
    }

    // Função para desenhar a borda ao redor do retângulo.
    private void DrawBorder(Rect rect)
    {
        Color borderColor = Color.black;
        float borderInset = borderThickness / 2;

        // Desenha as 4 bordas usando um método auxiliar.

        // Superior.
        DrawSideBorder(rect.x - borderInset, rect.y - borderInset, rect.width + borderThickness, borderThickness, borderColor);

        // Inferior.
        DrawSideBorder(rect.x - borderInset, rect.y + rect.height - borderInset, rect.width + borderThickness, borderThickness, borderColor);

        // Esquerda.
        DrawSideBorder(rect.x - borderInset, rect.y - borderInset, borderThickness, rect.height + borderThickness, borderColor);

        // Direita.
        DrawSideBorder(rect.x + rect.width - borderInset, rect.y - borderInset, borderThickness, rect.height + borderThickness, borderColor);
    }

    // Função auxiliar para desenhar um lado da borda.
    private void DrawSideBorder(float x, float y, float width, float height, Color color)
    {
        EditorGUI.DrawRect(new Rect(x, y, width, height), color);
    }

    // Função para desenhar os quadrados verdes nas 4 esquinas.
    private void DrawResizeHandles(Rect rect)
    {
        float size = handleSize;
        Vector2[] corners = new Vector2[4]
        {
            new(rect.x, rect.y), // Canto superior esquerdo.
            new(rect.x + rect.width, rect.y), // Canto superior direito.
            new(rect.x, rect.y + rect.height), // Canto inferior esquerdo.
            new(rect.x + rect.width, rect.y + rect.height) // Canto inferior direito.
        };

        // Desenha um quadrado verde em cada canto.
        foreach (Vector2 corner in corners)
        {
            // Desenha o quadrado verde em cada canto (ajustado pelo tamanho do quadrado).
            EditorGUI.DrawRect(new Rect(corner.x - size / 2, corner.y - size / 2, size, size), Color.green);
        }
    }

    // Função para determinar o ponto de referência de redimensionamento.
    private Vector2 GetResizePivot(Vector2 mousePos)
    {
        // Verifica se o clique foi dentro de qualquer quadrado verde.
        if (IsMouseInResizeHandle(mousePos, boxRect.x, boxRect.y)) // Canto superior esquerdo.
        {
            return new Vector2(boxRect.x, boxRect.y);
        }
        else if (IsMouseInResizeHandle(mousePos, boxRect.x + boxRect.width, boxRect.y)) // Canto superior direito.
        {
            return new Vector2(boxRect.x + boxRect.width, boxRect.y);
        }
        else if (IsMouseInResizeHandle(mousePos, boxRect.x, boxRect.y + boxRect.height)) // Canto inferior esquerdo.
        {
            return new Vector2(boxRect.x, boxRect.y + boxRect.height);
        }
        else if (IsMouseInResizeHandle(mousePos, boxRect.x + boxRect.width, boxRect.y + boxRect.height)) // Canto inferior direito.
        {
            return new Vector2(boxRect.x + boxRect.width, boxRect.y + boxRect.height);
        }

        return Vector2.zero; // Caso não tenha clicado em um quadrado verde.
    }

    // Função auxiliar para verificar se o mouse está dentro do quadrado verde de redimensionamento.
    private bool IsMouseInResizeHandle(Vector2 mousePos, float centerX, float centerY)
    {
        float halfSize = handleSize / 2;
        return mousePos.x >= centerX - halfSize && mousePos.x <= centerX + halfSize && mousePos.y >= centerY - halfSize && mousePos.y <= centerY + halfSize;
    }

    // Verifica se o mouse está dentro da área da borda preta (somente a borda, não o quadrado vermelho ou verde).
    private bool IsMouseOnBorder(Vector2 mousePos)
    {
        // Verifica se o mouse está dentro de uma área preta (borda) ao redor do quadrado vermelho.
        bool isOnTopBorder = mousePos.y >= boxRect.y - borderThickness && mousePos.y <= boxRect.y;
        bool isOnBottomBorder = mousePos.y >= boxRect.y + boxRect.height && mousePos.y <= boxRect.y + boxRect.height + borderThickness;
        bool isOnLeftBorder = mousePos.x >= boxRect.x - borderThickness && mousePos.x <= boxRect.x;
        bool isOnRightBorder = mousePos.x >= boxRect.x + boxRect.width && mousePos.x <= boxRect.x + boxRect.width + borderThickness;

        return isOnTopBorder || isOnBottomBorder || isOnLeftBorder || isOnRightBorder;
    }

    // Verifica se o mouse está em algum dos quadrados verdes.
    private bool IsMouseOnResizeHandle(Vector2 mousePos)
    {
        float size = handleSize / 2; // Meio do tamanho do quadrado verde (handle).

        // Verifica se o mouse está em qualquer um dos quadrados verdes (cantos).

        // Canto superior esquerdo.
        bool isTopLeft = mousePos.x >= boxRect.x - size && mousePos.x <= boxRect.x + size && mousePos.y >= boxRect.y - size && mousePos.y <= boxRect.y + size;

        // Canto superior direito.
        bool isTopRight = mousePos.x >= boxRect.x + boxRect.width - size && mousePos.x <= boxRect.x + boxRect.width + size && mousePos.y >= boxRect.y - size && mousePos.y <= boxRect.y + size;

        // Canto inferior esquerdo.
        bool isBottomLeft = mousePos.x >= boxRect.x - size && mousePos.x <= boxRect.x + size && mousePos.y >= boxRect.y + boxRect.height - size && mousePos.y <= boxRect.y + boxRect.height + size;

        // Canto inferior direito.
        bool isBottomRight = mousePos.x >= boxRect.x + boxRect.width - size && mousePos.x <= boxRect.x + boxRect.width + size && mousePos.y >= boxRect.y + boxRect.height - size && mousePos.y <= boxRect.y + boxRect.height + size;

        // Retorna verdadeiro se o mouse estiver em algum dos quadrados verdes.
        return isTopLeft || isTopRight || isBottomLeft || isBottomRight;
    }
}