// --- app.js ---
// Main application logic for Brain Buster (Hot-Seat Mode).

const App = {
    appElement: document.getElementById('app'),
    connection: null,
    
    // Transient state for the setup process
    setupState: {
        playerCount: 0,
        playerNames: [],
    },

    init() {
        this.setupSignalR();
        this.appElement.addEventListener('click', this.handleAppClick.bind(this));
    },

    setupSignalR() {
        this.connection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();

        this.connection.on("GameStateChanged", (state) => {
            console.log("Game state changed:", state);
            this.render(state);
        });

        this.connection.start()
            .then(() => API.getGameState())
            .then(state => this.render(state))
            .catch(err => console.error("SignalR Connection Error: ", err));
    },
    
    async render(gameState) {
        let html = '';
        if (gameState) {
            switch(gameState.status) {
                case 'InProgress':
                    html = UI.renderQuestion(gameState);
                    break;
                case 'Summary':
                    html = UI.renderSummary(gameState);
                    break;
                case 'Lobby':
                default:
                    // If we are in the lobby, we start the setup flow
                    html = UI.renderPlayerCount();
                    break;
            }
        } else {
             // Initial load
            html = UI.renderPlayerCount();
        }
        this.appElement.innerHTML = html;
    },

    async handleAppClick(e) {
        const target = e.target;

        // --- Step 1: Player Count Submission ---
        if (target.id === 'player-count-submit') {
            const input = document.getElementById('player-count-input');
            const count = parseInt(input.value, 10);
            if (count >= 1 && count <= 4) {
                this.setupState.playerCount = count;
                this.appElement.innerHTML = UI.renderPlayerNames(this.setupState.playerCount);
            } else {
                alert('Bitte eine Zahl zwischen 1 und 4 eingeben.');
            }
        }

        // --- Step 2: Player Name Submission ---
        if (target.id === 'next-to-categories-btn') {
            const inputs = Array.from(document.querySelectorAll('#player-names input'));
            this.setupState.playerNames = inputs.map(input => input.value.trim()).filter(name => name);
            
            if (this.setupState.playerNames.length !== this.setupState.playerCount) {
                alert('Bitte für jeden Spieler einen Namen eingeben.');
                return;
            }
            
            const categories = await API.getCategories();
            this.appElement.innerHTML = UI.renderCategories(categories);
        }

        // --- Step 3: Category Selection & Game Start ---
        if (target.id === 'start-game-btn') {
            const selectedCategories = Array.from(document.querySelectorAll('input[type=checkbox]:checked')).map(cb => cb.value);
            await API.startGame(this.setupState.playerNames, selectedCategories);
        }
        
        // --- In-Game: Answering ---
        if (target.classList.contains('answer-btn')) {
            target.parentElement.parentElement.querySelectorAll('.answer-btn').forEach(btn => btn.disabled = true);
            await API.submitAnswer(parseInt(target.dataset.index, 10));
        }

        // --- Summary: Play Again ---
        if (target.id === 'play-again-btn') {
            await API.resetGame();
        }
    }
};

App.init();
