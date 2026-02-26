# Draw.io sequence диаграми – LeafMap Insights

Sequence и use case диаграмите са в **draw.io (diagrams.net)** XML формат по **UML sequence** конвенция:

- **Участници** – малки правоъгълници (кутии) отгоре, както в примерите за sequence диаграми.
- **Lifelines** – вертикални линии под всеки участник (време тече отгоре-надолу).
- **Съобщения** – хоризонтални стрелки между lifeline-овете: плътна стрелка = извикване, пунктирна отворена = отговор.

## Класически UML вид в draw.io

За точно като в примерите (пунктирни lifeline-ове, activation кутии) отворете диаграмата в draw.io и:

1. **Arrange → Insert → Template** → търсете „sequence“ и изберете шаблон, или  
2. **More Shapes** (долу вляво) → **Software** → включете **UML** и **UML 2.5** → плъзнете формата **Lifeline** от палитрата и подравнете участниците.

Съдържанието (участници и съобщения) е вече в тези файлове; при желание можете да замените lifeline-овете с вградените UML форми.

## Файлове

| Файл | Проект |
|------|--------|
| `LeafMapInsightsStaticWeb-sequence.drawio` | Статичен HTML/JS клиент |
| `LeafMapInsightsNodeClient-sequence.drawio` | Node.js уеб сайт |
| `MAUILeafMapInsights-sequence.drawio` | MAUI приложение |
| `LeafMapInsights-use-case.drawio` | Use case – през коя апликация какво можем да правим |
| **`LeafMapInsights-component-overview.drawio`** | **Компонентни диаграми** – 5 страници: обзор на системата, Auth API, Data API, Node.js клиент, MAUI клиент |

## Как да отворите

1. **draw.io Desktop** или **https://app.diagrams.net/** (диаграми.нет)
2. **File → Open from → Device** и изберете съответния `.drawio` файл.
3. Или преименувайте разширението на `.xml` и отворете с **File → Import from → Device** ако приложението не разпознава `.drawio`.

Всяка диаграма съдържа **swimlanes** (колони) за участниците и **стрелки** с надписи за съобщенията между тях. Можете да редактирате стилове, размери и текстове директно в draw.io.

Мermaid версиите са в [../sequence-diagrams.md](../sequence-diagrams.md).
