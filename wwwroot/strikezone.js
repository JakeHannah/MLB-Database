// wwwroot/scripts/pitchviz.js

window.pitchViz = (() => {
    let scene, camera, renderer, ball, trail, strikeZoneMesh, animationId;
    let trajectoryPoints = [];

    // --- Initialization once at page load ---
    function initPitchViz(pbpJson) {
        const data = JSON.parse(pbpJson);
        // extract the most recent play’s last pitch...
        const plays = data.AllPlays;
        if (!plays || !plays.length) return;
        const lastPlay = plays[plays.length - 1];
        // find the last pitch event in that play
        const pitchEvent = lastPlay.PlayEvents
            .filter(e => e.IsPitch)
            .slice(-1)[0];
        if (!pitchEvent) return;

        // set up Three.js scene (similar to your pitch.html init)
        const canvas = document.getElementById('pitchCanvas');
        renderer = new THREE.WebGLRenderer({ canvas, antialias: true });
        renderer.setClearColor(0xe0f0e0);
        scene = new THREE.Scene();
        camera = new THREE.PerspectiveCamera(60, canvas.clientWidth / canvas.clientHeight, 0.1, 1000);
        camera.position.set(0, 4, 8);
        camera.lookAt(0, 2, 0);
        scene.add(new THREE.AmbientLight(0xffffff, 0.7));
        const dir = new THREE.DirectionalLight(0xffffff, 0.9);
        dir.position.set(5, 15, 10);
        scene.add(dir);

        // ball
        ball = new THREE.Mesh(
            new THREE.SphereGeometry(0.12, 16, 16),
            new THREE.MeshStandardMaterial({ color: 0xffffff })
        );
        scene.add(ball);

        // trail line
        trail = new THREE.Line(
            new THREE.BufferGeometry(),
            new THREE.LineBasicMaterial({ color: 0xff0000 })
        );
        scene.add(trail);

        // … you can also add strikeZoneMesh, plate, mound, etc …

        // then render the first frame and start animation
        updatePitch(pitchEvent);
    }

    // --- Called on every 15s refresh with fresh JSON ---
    function updatePitchViz(pbpJson) {
        const data = JSON.parse(pbpJson);
        const plays = data.AllPlays;
        if (!plays || !plays.length) return;
        const lastPlay = plays[plays.length - 1];
        const pitchEvent = lastPlay.PlayEvents
            .filter(e => e.IsPitch)
            .slice(-1)[0];
        if (!pitchEvent) return;

        // cancel any in‑flight animation
        if (animationId) cancelAnimationFrame(animationId);
        updatePitch(pitchEvent);
    }

    // shared logic to set up initial state & animate
    function updatePitch(event) {
        const coords = event.PitchData.Coordinates;
        const plateTime = event.PitchData.PlateTime || 0.5;
        // map coords from MLB→Three.js (x0,y0,z0 etc)
        const initY = coords.Y0 > 0 ? coords.Y0 : 55;
        const pos0 = new THREE.Vector3(coords.X0, coords.Z0, -initY);
        const vel0 = new THREE.Vector3(coords.VX0, coords.VZ0, coords.VY0);
        const acc = new THREE.Vector3(coords.AX, coords.AZ, coords.AY || 0);

        ball.position.copy(pos0);
        trajectoryPoints = [pos0.clone()];
        trail.geometry.dispose();
        trail.geometry = new THREE.BufferGeometry().setFromPoints(trajectoryPoints);

        let startTime = performance.now();
        function animate(now = performance.now()) {
            animationId = requestAnimationFrame(animate);
            const t = (now - startTime) / 1000;
            if (t <= plateTime) {
                // pos = pos0 + vel0*t + 0.5*acc*t^2
                const p = pos0.clone()
                    .addScaledVector(vel0, t)
                    .addScaledVector(acc, 0.5 * t * t);
                ball.position.copy(p);

                if (trajectoryPoints.length === 0
                    || p.distanceTo(trajectoryPoints[trajectoryPoints.length - 1]) > 0.1) {
                    trajectoryPoints.push(p.clone());
                    if (trajectoryPoints.length > 150) trajectoryPoints.shift();
                    trail.geometry.setFromPoints(trajectoryPoints);
                }
            } else {
                // keep at plate
                cancelAnimationFrame(animationId);
            }
            renderer.render(scene, camera);
        }
        animate();
    }

    return {
        initPitchViz,
        updatePitchViz
    };
})();
