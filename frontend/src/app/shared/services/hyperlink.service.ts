import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class HyperlinkService {
  private readonly urlRegex = /(https?:\/\/[^\s<>"'()[\]{}]+)/gi;
  private readonly allowedSchemes = ['http:', 'https:'];
  private readonly maxUrlLength = 2000;

  convertTextToHtml(text: string): string {
    if (!text) return '';

    return text.replace(this.urlRegex, (url) => {
      const sanitizedUrl = this.sanitizeUrl(url);
      if (sanitizedUrl === '#') {
        return url;
      }

      const displayUrl = url.length > 50 ? url.substring(0, 47) + '...' : url;
      return `<a href="${sanitizedUrl}" target="_blank" rel="noopener noreferrer" class="note-link">${displayUrl}</a>`;
    });
  }

  extractUrls(text: string): string[] {
    if (!text) return [];

    const matches = text.match(this.urlRegex);
    return matches ? [...new Set(matches)] : [];
  }

  validateUrl(url: string): { isValid: boolean; error?: string } {
    if (!url) {
      return { isValid: false, error: 'URL is required' };
    }

    if (url.length > this.maxUrlLength) {
      return { isValid: false, error: 'URL is too long' };
    }

    try {
      const parsed = new URL(url);

      if (!this.allowedSchemes.includes(parsed.protocol)) {
        return {
          isValid: false,
          error: 'Only HTTP and HTTPS URLs are allowed',
        };
      }

      if (
        parsed.protocol === 'javascript:' ||
        parsed.protocol === 'data:' ||
        parsed.protocol === 'vbscript:'
      ) {
        return { isValid: false, error: 'Unsafe URL scheme detected' };
      }

      return { isValid: true };
    } catch {
      return { isValid: false, error: 'Invalid URL format' };
    }
  }

  private sanitizeUrl(url: string): string {
    try {
      const parsed = new URL(url);

      if (!this.allowedSchemes.includes(parsed.protocol)) {
        return '#';
      }

      if (
        parsed.protocol === 'javascript:' ||
        parsed.protocol === 'data:' ||
        parsed.protocol === 'vbscript:'
      ) {
        return '#';
      }

      return url;
    } catch {
      return '#';
    }
  }

  validateContent(content: string): { isValid: boolean; errors: string[] } {
    const urls = this.extractUrls(content);
    const errors: string[] = [];

    for (const url of urls) {
      const validation = this.validateUrl(url);
      if (!validation.isValid && validation.error) {
        errors.push(`Invalid URL "${url}": ${validation.error}`);
      }
    }

    return {
      isValid: errors.length === 0,
      errors,
    };
  }
}
