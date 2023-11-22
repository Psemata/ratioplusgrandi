# Alpha-Beta Othello - AI Report
## Introduction
As part of the artificial intelligence course, it was required to implement a complete Alpha-Beta algorithm for an Othello game. The purpose of this AI is to compete in a tournament organized by the instructor, where each group in the class will submit its "champion." The best-performing AI will be determined based on the number of victories.

## Alpha-Beta
Alpha-Beta is a pruning technique used to reduce the number of nodes explored by the Min-Max algorithm. The primary goal of this algorithm and its pruning is to apply game theory to minimize losses and maximize gains in a turn, thus making the best possible move.

## Evaluation Function
The algorithm used for this project, as described above, includes a crucial element known as the evaluation function. This component manages the logic and how the nodes in the tree are judged. Various criteria, corresponding to key aspects of the game state, are calculated and summed to obtain a final score.

### Evaluation Criteria
After thorough research, the group decided to use five specific criteria to evaluate and assess the game board state:

1. "nToken" corresponds to the number of tokens the player has on the board.
2. "cMobility" represents the current mobility: the number of possible moves in the current board state.
3. "pMobility" represents potential mobility: the number of empty squares adjacent to the opponent's tokens -> the number of potential future moves.
4. "placement" assigns values to different strategic locations on the board, such as corners or edges.
5. "stability" represents the stability of positions on the board where a token is likely to be flipped.

The selection of these values was based on the analysis of various documents containing information on different Othello evaluation types [1][2][3][4]. Key factors included strategic point capture, such as corners of the board, the number of available moves, and the stability of tokens on the board.

These criteria are calculated and maximized or minimized based on the current player's turn. If the white player is playing, they will maximize their points and minimize those of the black player, and vice versa. Additionally, we observed that the evaluation function performed better when penalizing the capture of many points at the beginning of the game. Therefore, the AI focuses on strategic point capture early on, leading to a significant point capture when there are fewer empty squares.

Finally, each criterion is amplified by a specific factor according to its importance. For example, since point capture is less critical at the beginning, its coefficient will be smaller than the others. The final coefficients for the evaluation function are as follows:

- nToken: -3
- placement: 10
- pMobility: 7
- cMobility: 15
- stability: 10

## Tests
The coefficients mentioned above were determined through multiple tests. The "Ratio + Nul + Grandi" AI played numerous games against other AIs for training purposes.

The AIs it trained against include:
- ValatLeuba
- LatinoRosca
- LeGolem
- YusukeTakahasi
- La anusrompilo

The group's AI wins the majority of matches, but more often when playing as black.

## Conclusion
In conclusion, the "Ratio + Nul + Grandi" AI underwent extensive testing, demonstrating a stable state where it is victorious 4/5 of the time when playing as black and 2/5 when playing as white. It is well-prepared to compete in the upcoming tournament.

## References 
[1] [Mini-Othello – Yunpeng Li and Dobo Radichkov](https://www.cs.cornell.edu/~yuli/othello/othello.html)  
[2] [The Development of a World Class Othello Program](https://matthieu-zimmer.net/~matthieu/courses/python/othello.pdf)  
[3] [THE EVOLUTION OF STRONG OTHELLO PROGRAMS](https://link.springer.com/content/pdf/10.1007/978-0-387-35660-0_10.pdf)  
[4] [An Evaluation Function for Othello Based on Statistics](https://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.49.7258&rep=rep1&type=pdf)  
