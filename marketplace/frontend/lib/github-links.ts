export function parseGitHubRepo(repositoryUrl: string): {
  owner: string
  repo: string
  base: string
  readmeUrl: string
  issuesUrl: string
} | null {
  try {
    const url = new URL(repositoryUrl)
    if (!url.hostname.includes("github.com")) {
      return null
    }
    const parts = url.pathname.split("/").filter(Boolean)
    if (parts.length < 2) {
      return null
    }
    const owner = parts[0]
    const repo = parts[1].replace(/\.git$/i, "")
    const base = `https://github.com/${owner}/${repo}`
    return {
      owner,
      repo,
      base,
      readmeUrl: `${base}#readme`,
      issuesUrl: `${base}/issues`,
    }
  } catch {
    return null
  }
}
