# Alpha-Beta Othello - Rapport IA
## Introduction
Dans le cadre du cours d'intelligence artificielle, il a été demandé de réaliser un algorithme Alpha-Beta complet pour un jeu d'othello. Cette IA a pour but de se "battre" dans un tournoi organisé par l'enseignant où chaque groupe de la classe y enverra son "champion".
À la fin de ces batailles, la meilleure IA sera désignée par son nombre de victoires.

## Alpha-Beta
L'Alpha-Beta est un élagage qui permet de réduire le nombre de noeuds étudiés par l'algorithme Min-Max. Le but premier de cet algorithme, et de cet élagage, est de s'appliquer à la théorie des jeux pour minimiser les pertes et maximiser les gains d'un tour pour ainsi jouer le meilleur coup possible.

## Fonction d'évaluation
L'algorithme utilisé pour ce projet et décrit plus haut possède un élément très important qui est la fonction d'évaluation. C'est cette partie qui va gérer la logique et comment les noeuds de l'arbre seront jugés.
Différents critères correspondant à des points importants de la partie sont calculés et additionnés pour récupérer un score final.

### Critères d'évaluation
Après plusieurs recherches, le groupe a décidé d'utiliser 5 critères bien précis pour évaluer et juger l'état du plateau de jeu :

1. "nToken" correspond au nombre de jetons du joueur disponibles sur le terrain
2. "cMobility" correspond à la mobilité actuelle : le nombre de mouvements possibles dans l'état du plateau.
3. "pMobility" correspond à la mobilité potentielle : le nombre de cases vides adjacentes aux jetons de l'adversaire -> le nombre de potentiels déplacements futurs
4. "placement" correspond à des valeurs attribuées aux différents emplacements stratégiques du tableau : les coins ou les bords du plateau.
6. "stability" correspond aux emplacements du plateau où un jeton à peu ou beaucoup de chances d'être retournées.

Le choix de ces valeurs a été fait par l'analyse de plusieurs documents regroupant des informations sur les différents types d'évaluation d'Othello [1][2][3][4].
Les plus importants étaient la prise de points stratégiques comme les coins du plateau, le nombre de mouvements disponibles ou encore la stabilité des jetons sur le plateau.
Ces différents critères sont calculés et maximisés ou minimisés selon le joueur actuel du tour. Si le joueur blanc joue, il va maximiser ses points et minimiser ceux du joueur noir et vice-versa.
De plus, nous avons remarqué que la fonction d'évaluation était plus performante lorsque nous pénalisions la prise de beaucoup de points en début de partie.
Ainsi, l'IA va se concentrer sur la prise de point stratégique au début pour finir par une grande prise de points lorsqu'il ne reste plus beaucoup de cases vides.

Finalement, chaque critère est amplifié par un certain facteur selon son importance. En effet, la prise de points étant moins importante au début, son coefficient sera plus petit que les autres.
Voici les différents coefficients finaux de la fonction d'évaluation :

- nToken : -3
- placement : 10
- pMobility : 7
- cMobility : 15
- stability : 10

## Tests
Les coefficients notés ci-dessus ont été trouvés via de multiples tests. L'IA "Ratio + Nul + Grandi" a joué de nombreuses parties contre d'autres IA afin de s'entraîner.

Les IAs contre lesquelles elle s'est entraînée sont :
- ValatLeuba
- LatinoRosca
- LeGolem
- YusukeTakahasi
- La anusrompilo

L'IA du groupe gagne sur une majorité des matches, mais plus en étant noir qu'en étant blanc

## Conclusion
En conclusion, l'IA "Ratio + Nul + Grandi" a subi de multiples tests énumérés ci-dessus qui démontrent un état stable où elle est victorieuse 4/5 du temps en étant noire et 2/5 en étant blanche et est prête à battre tout le monde lors du tournoi.

## Références 
[1] [Mini-Othello – Yunpeng Li and Dobo Radichkov](https://www.cs.cornell.edu/~yuli/othello/othello.html)
[2] [The Development of a World Class Othello Program](https://matthieu-zimmer.net/~matthieu/courses/python/othello.pdf)
[3] [THE EVOLUTION OF STRONG OTHELLO PROGRAMS](https://link.springer.com/content/pdf/10.1007/978-0-387-35660-0_10.pdf)
[4] [An Evaluation Function for Othello Based on Statistics](https://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.49.7258&rep=rep1&type=pdf)