import http from "k6/http";
import { check, sleep } from "k6";

export let options = {
  stages: [
    { duration: "2m", target: 5 }, // Warm up
    { duration: "5m", target: 20 }, // Stay at 20 users
    { duration: "2m", target: 0 }, // Cool down
  ],
};

export default function () {
  // Test various endpoints
  let endpoints = [
    "http://localhost:8080/",
    "http://localhost:8080/account/login",
    "http://localhost:8080/games",
    "http://localhost:8080/dashboard",
  ];

  let endpoint = endpoints[Math.floor(Math.random() * endpoints.length)];

  let response = http.get(endpoint, {
    headers: { "User-Agent": "K6-Load-Test" },
  });

  check(response, {
    "status is 200 or 302": (r) => r.status === 200 || r.status === 302,
    "response time < 2s": (r) => r.timings.duration < 2000,
  });

  sleep(1);
}
