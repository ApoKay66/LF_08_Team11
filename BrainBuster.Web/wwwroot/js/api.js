// --- api.js ---
// Handles all communication with the backend API for the hot-seat mode.

const API = {
    async getGameState() {
        const response = await fetch('/api/game/state');
        return response.json();
    },
    
    async getCategories() {
        const response = await fetch('/api/game/categories');
        return response.json();
    },

    async startGame(playerNames, categories) {
        await fetch('/api/game/start', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ playerNames, categories })
        });
    },
    
    async submitAnswer(choiceIndex) {
        await fetch('/api/game/answer', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ choiceIndex })
        });
    },

    async resetGame() {
        await fetch('/api/game/reset', {
            method: 'POST'
        });
    }
};
