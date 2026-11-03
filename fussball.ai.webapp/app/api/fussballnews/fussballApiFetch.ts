type FetchOptions = RequestInit & { json?: any };

export async function fussballApiFetch(
  url: string,
  options: FetchOptions = {}
) {
  const { json, headers, ...rest } = options;
  const fetchOptions: RequestInit = {
    ...rest,
    headers: {
      "Content-Type": "application/json",
      ...headers,
    },
    body: json ? JSON.stringify(json) : options.body,
  };
  const res = await fetch(`https://localhost:7263${url}`, fetchOptions);
  if (!res.ok) throw new Error("Network error");
  return res.json();
}