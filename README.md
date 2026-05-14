Blue Friday
============

Autores:
- José Silva 21076
- Nuno Alves 31181
- Joana Sousa 34976

Ideia geral
------------
O jogo inclui combate contra inimigos de combate corpo a corpo e inimigos de ataque à distância, projéteis, recolha de experiência.
A lógica principal encontra-se em `Game1.cs`, responsável pelo loop do jogo, renderização e atualização das entidades.

Pontos fortes
------------
- Organização por Classes: O jogo está dividido em várias classes com responsabilidades separadas:

Enemy.cs → lógica base dos inimigos;
BossEnemy.cs → boss com animação;
RangedEnemy.cs → inimigo com ataques à distância;
PlayerAttack.cs → sistema de ataque do player;
ExperienceGem.cs → sistema de experiência;
Wall.cs e BrownWall.cs → colisões e obstáculos.

(Isto ajuda bastante na manutenção do código)

- Uso de Herança
Exemplo:

```csharp
public class RangedEnemy : Enemy
{
    private float _attackTimer = 0.0f;
    private float _attackInterval = 2.5f;
}
```
A classe `RangedEnemy` herda funcionalidades da classe `Enemy`, evitando repetição de código.

- Código organizado por responsabilidade: cada entidade tem a sua própria classe, facilitando evolução e manutenção.
- Target .NET 8, aproveitando as melhorias de desempenho e APIs recentes.
- Projeto pequeno e direto, bom para prototipagem e aprendizagem.


Pontos fracos
------------



A classe `Game1.cs` concentra grande parte da lógica principal do jogo, incluindo atualização de entidades, renderização, carregamento de conteúdos e controlo geral do gameplay.
```csharp
protected override void Update(GameTime gameTime)
{
    player.Update();
    enemy.Update();
    boss.Update();
    projectile.Update();
}
```
À medida que o projeto cresce, esta classe pode tornar-se demasiado grande e difícil de gerir.


- Existem classes muito semelhantes, como `Wall` e `BrownWall`, que possuem funcionalidades parecidas.
```csharp
public class Wall
{
    public Rectangle Bounds;
}
```

```csharp
public class BrownWall
{
    public Rectangle Bounds;
}
```
A lógica poderia ser reutilizada através de uma classe base genérica para evitar repetição de código.
  
- Mais comentários explicativos no código: Embora existam alguns comentários, certas partes complexas do combate e movimentação poderiam
estar melhor documentadas, como por exemplo: cálculos de direção, colisões, animações e gestão de mapas.
- Carregamento de recursos diretamente no código: Alguns caminhos de imagens e sons podem estar definidos diretamente no código, o que dificulta futuras alterações na estrutura das pastas.
```csharp
Content.Load<Texture2D>("images/player");
```
Caso o nome da pasta ou do ficheiro seja alterado, o jogo pode deixar de carregar corretamente os recursos.

Estrutura:
------------

`jogo\Game1.cs`
Loop principal do jogo e renderização. Responsável por: carregar conteúdos, atualizar lógica, desenhar elementos, controlar mapas e gerir inimigos.

`jogo\Program.cs`
Ponto de entrada

`jogo\Enemy.cs`
Classe base para inimigos: Inclui: vida, movimento, animação, deteção de colisão.

`jogo\BossEnemy.cs`
Inimigo mais forte. Especialização da classe Enemy. Inclui: animações próprias, lógica de boss, múltiplos frames.

`jogo\RangedEnemy.cs`
Inimigos à distância. Inimigo que dispara projéteis. Inclui: temporizador de ataque, criação de munições e IA simples de perseguição.

`jogo\RangedEnemyBullet.cs`
Projéteis de inimigos à distância

`jogo\PlayerAttack.cs`
Lógica de ataques do jogador. Controla: ataques, direção, duração, dano e alcance.

`jogo\ExperienceGem.cs`
Responsável pelas gemas de experiência recolhidas pela personagem.

`jogo\Wall.cs`
Parede genérica

`jogo\BrownWall.cs`
Variante de parede
