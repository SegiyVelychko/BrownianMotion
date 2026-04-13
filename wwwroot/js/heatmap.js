window.drawHeatmap = function (canvasId, cells, rows, cols, maxDensity) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const parent = canvas.parentElement;
    const size = Math.min(parent.clientWidth - 24, parent.clientHeight - 48) || 480;
    canvas.width = size;
    canvas.height = size;

    const ctx = canvas.getContext('2d');
    const cellW = size / cols;
    const cellH = size / rows;

    ctx.clearRect(0, 0, size, size);

    for (let r = 0; r < rows; r++) {
        for (let c = 0; c < cols; c++) {
            const val = cells[r * cols + c];

            if (val < 0) {
                const depth = Math.min(Math.abs(val) / 20, 1);
                const g = Math.round(200 + depth * 55);
                const b = Math.round(220 + depth * 35);
                ctx.fillStyle = `rgb(0,${g},${b})`;
            } else {
                const norm = maxDensity > 0 ? val / maxDensity : 0;
                ctx.fillStyle = thermalColor(norm);
            }

            ctx.fillRect(c * cellW, r * cellH, cellW, cellH);
        }
    }

    if (cellW > 8) {
        ctx.strokeStyle = 'rgba(255,255,255,0.04)';
        ctx.lineWidth = 0.5;
        for (let r = 0; r < rows; r++)
            for (let c = 0; c < cols; c++)
                ctx.strokeRect(c * cellW + 0.25, r * cellH + 0.25, cellW - 0.5, cellH - 0.5);
    }
};

function thermalColor(t) {
    const stops = [
        [0.00, 2, 2, 8],
        [0.20, 60, 5, 100],
        [0.45, 180, 0, 30],
        [0.65, 230, 90, 0],
        [0.82, 240, 200, 0],
        [1.00, 255, 255, 220],
    ];
    if (t <= 0) return 'rgb(2,2,8)';
    if (t >= 1) return 'rgb(255,255,220)';
    let i = 0;
    while (i < stops.length - 2 && t > stops[i + 1][0]) i++;
    const [t0, r0, g0, b0] = stops[i];
    const [t1, r1, g1, b1] = stops[i + 1];
    const f = (t - t0) / (t1 - t0);
    return `rgb(${lerp(r0, r1, f)},${lerp(g0, g1, f)},${lerp(b0, b1, f)})`;
}

function lerp(a, b, t) { return Math.round(a + (b - a) * t); }
