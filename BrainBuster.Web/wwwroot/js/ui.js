// --- ui.js ---
// Renders the UI for the local hot-seat multiplayer game in German.

const UI = {
    renderPlayerCount() {
        return `
            <div class="text-center">
                <h1>Lokaler Mehrspieler</h1>
                <p class="lead">Wie viele Personen spielen?</p>
                <div class="mt-4">
                    <input type="number" id="player-count-input" class="form-control form-control-lg text-center" min="1" max="4" placeholder="Anzahl eingeben (1-4)" />
                </div>
                <button id="player-count-submit" class="btn btn-primary btn-lg mt-3">Weiter</button>
            </div>
        `;
    },

    renderPlayerNames(playerCount) {
        let inputs = '';
        for (let i = 1; i <= playerCount; i++) {
            inputs += `<input type="text" class="form-control mb-2" placeholder="Spieler ${i} Name" />`;
        }
        return `
            <div class="text-center">
                <h2>Spielernamen eingeben</h2>
                <div id="player-names" class="mt-4">
                    ${inputs}
                </div>
                <button id="next-to-categories-btn" class="btn btn-primary mt-3">Weiter</button>
            </div>
        `;
    },

    renderCategories(categories) {
        return `
            <div class="text-center">
                <h2>Kategorien auswählen</h2>
                <p>Wähle Kategorien aus oder lasse das Feld frei für einen Mix.</p>
                <div class="form-check text-start my-4" style="max-height: 200px; overflow-y: auto;">
                    ${categories.map(cat => `
                        <div>
                            <input class="form-check-input" type="checkbox" value="${cat}" id="cat-${cat}">
                            <label class="form-check-label" for="cat-${cat}">${cat}</label>
                        </div>
                    `).join('')}
                </div>
                <button id="start-game-btn" class="btn btn-primary btn-lg">Spiel starten!</button>
            </div>
        `;
    },

    renderQuestion(state) {
        const turn = state.activeTurn;
        return `
            <div class="text-center">
                <h2>${turn.playerName} ist am Zug!</h2>
                <p class="lead">${turn.question.questionText}</p>
                <hr />
                <div class="row mt-4">
                    ${turn.shuffledAnswers.map((answer, index) => `
                        <div class="col-md-6 mb-2">
                            <button class="btn btn-info w-100 p-3 answer-btn" data-index="${index}">
                                ${answer.answerText}
                            </button>
                        </div>
                    `).join('')}
                </div>
            </div>
        `;
    },

    renderSummary(state) {
        return `
            <div class="text-center">
                <h1 class="display-4">Spiel vorbei!</h1>
                <p>Hier ist die Auswertung:</p>
            </div>
            <table class="table table-striped mt-4">
                <thead>
                    <tr>
                        <th>Rang</th>
                        <th>Spieler</th>
                        <th>Punkte</th>
                        <th>Highscore</th>
                    </tr>
                </thead>
                <tbody>
                    ${state.summary.map((player, index) => `
                        <tr>
                            <td>${index + 1}</td>
                            <td>${player.name}</td>
                            <td>${player.score}</td>
                            <td>${player.highScore}</td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
            <div class="text-center mt-4">
                <button id="play-again-btn" class="btn btn-primary me-2">Nochmal spielen</button>
                <button id="manage-players-btn" class="btn btn-secondary">Spieler verwalten</button>
            </div>
        `;
    },

    renderGlobalHighscores(highscores) {
        if (!highscores || highscores.length === 0) {
            return '<p class="text-center">Noch keine Einträge vorhanden.</p>';
        }

        return `
            <table class="table table-hover">
                <thead class="table-light">
                    <tr>
                        <th scope="col">#</th>
                        <th scope="col">Name</th>
                        <th scope="col">Bestleistung (Punkte)</th>
                    </tr>
                </thead>
                <tbody>
                    ${highscores.map((p, index) => `
                        <tr>
                            <th scope="row">${index + 1}</th>
                            <td>${p.name}</td>
                            <td>${p.highScore}</td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        `;
    }
};
