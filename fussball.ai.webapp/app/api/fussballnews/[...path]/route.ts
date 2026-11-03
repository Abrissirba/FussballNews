import { NextRequest, NextResponse } from 'next/server';

const BACKEND_BASE_URL = 'http://localhost:5025';

export async function handler(req: NextRequest, contextPromise: Promise<{ params: { path: string[] } }>) {
  const { params } = await contextPromise;

  const path = params.path ? params.path.join('/') : '';
  const url = `${BACKEND_BASE_URL}/${path}`;

  // Prepare headers, removing 'host' to avoid issues
  const headers = new Headers(req.headers);
  headers.delete('host');

  // Prepare fetch options
  const fetchOptions: RequestInit = {
    method: req.method,
    headers,
    body: req.method !== 'GET' && req.method !== 'HEAD' ? await req.text() : undefined,
    // credentials, mode, etc. can be added if needed
  };

  // Forward the request
  const apiRes = await fetch(url, fetchOptions);
  const contentType = apiRes.headers.get('content-type') || '';
  let body;
  if (contentType.includes('application/json')) {
    body = await apiRes.json();
    return NextResponse.json(body, { status: apiRes.status });
  } else {
    body = await apiRes.text();
    return new NextResponse(body, {
      status: apiRes.status,
      headers: { 'content-type': contentType },
    });
  }
}

export { handler as GET, handler as POST, handler as PUT, handler as DELETE, handler as PATCH, handler as OPTIONS, handler as HEAD };