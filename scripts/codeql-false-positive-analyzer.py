#!/usr/bin/env python3
"""
CodeQL False Positive Analyzer for C# Game Development
Specifically designed for GLTron Mobile - MonoGame/Android patterns
"""

import json
import sys
import os
import re
from typing import Dict, List, Tuple, Optional

class CodeQLFalsePositiveAnalyzer:
    """AI-powered false positive detection for C# game code"""
    
    def __init__(self, sarif_file: str):
        self.sarif_file = sarif_file
        self.false_positive_patterns = self._load_game_patterns()
        
    def _load_game_patterns(self) -> Dict[str, List[Dict]]:
        """Load game-specific false positive patterns"""
        return {
            "monogame_patterns": [
                {
                    "rule_pattern": r"cs/unused-local-variable",
                    "message_pattern": r"gameTime|spriteBatch|graphics",
                    "reason": "MonoGame framework variables (required by game loop)"
                },
                {
                    "rule_pattern": r"cs/empty-catch-block",
                    "file_pattern": r".*Content.*\.cs",
                    "reason": "Content loading error handling (MonoGame pattern)"
                }
            ],
            "android_patterns": [
                {
                    "rule_pattern": r"cs/unused-method",
                    "message_pattern": r"OnCreate|OnResume|OnPause|OnDestroy",
                    "reason": "Android lifecycle methods (required by framework)"
                },
                {
                    "rule_pattern": r"cs/unused-field",
                    "file_pattern": r".*Activity.*\.cs",
                    "reason": "Android Activity fields (framework requirements)"
                }
            ],
            "opengl_patterns": [
                {
                    "rule_pattern": r"cs/resource-not-disposed",
                    "message_pattern": r"GL|OpenGL|Texture|Buffer",
                    "reason": "OpenGL resource management (handled by graphics driver)"
                },
                {
                    "rule_pattern": r"cs/unused-local-variable",
                    "file_pattern": r".*Graphics.*\.cs|.*Renderer.*\.cs",
                    "reason": "Graphics rendering variables (OpenGL state management)"
                }
            ],
            "game_engine_patterns": [
                {
                    "rule_pattern": r"cs/unused-method",
                    "message_pattern": r"Update|Draw|LoadContent|Initialize",
                    "reason": "Game engine lifecycle methods (required by MonoGame)"
                },
                {
                    "rule_pattern": r"cs/dead-code",
                    "file_pattern": r".*AI.*\.cs|.*Player.*\.cs",
                    "reason": "Game logic branches (conditional on game state)"
                }
            ]
        }
    
    def analyze_sarif_results(self) -> Dict:
        """Analyze SARIF results for false positives"""
        try:
            with open(self.sarif_file, 'r') as f:
                sarif_data = json.load(f)
        except FileNotFoundError:
            return {"error": "SARIF file not found", "total_issues": 0}
        except json.JSONDecodeError:
            return {"error": "Invalid SARIF format", "total_issues": 0}
        
        analysis_results = {
            "total_issues": 0,
            "false_positives": 0,
            "valid_issues": 0,
            "false_positive_details": [],
            "valid_issue_details": [],
            "pattern_matches": {}
        }
        
        runs = sarif_data.get('runs', [])
        
        for run in runs:
            results = run.get('results', [])
            analysis_results["total_issues"] += len(results)
            
            for result in results:
                rule_id = result.get('ruleId', '')
                message = result.get('message', {}).get('text', '')
                locations = result.get('locations', [])
                
                # Check for false positive patterns
                fp_match = self._check_false_positive_patterns(rule_id, message, locations)
                
                if fp_match:
                    analysis_results["false_positives"] += 1
                    analysis_results["false_positive_details"].append({
                        "rule_id": rule_id,
                        "message": message[:100] + "..." if len(message) > 100 else message,
                        "reason": fp_match["reason"],
                        "pattern_type": fp_match["pattern_type"],
                        "confidence": fp_match["confidence"]
                    })
                    
                    # Track pattern matches
                    pattern_type = fp_match["pattern_type"]
                    if pattern_type not in analysis_results["pattern_matches"]:
                        analysis_results["pattern_matches"][pattern_type] = 0
                    analysis_results["pattern_matches"][pattern_type] += 1
                else:
                    analysis_results["valid_issues"] += 1
                    analysis_results["valid_issue_details"].append({
                        "rule_id": rule_id,
                        "message": message[:100] + "..." if len(message) > 100 else message,
                        "severity": result.get('level', 'unknown')
                    })
        
        return analysis_results
    
    def _check_false_positive_patterns(self, rule_id: str, message: str, locations: List) -> Optional[Dict]:
        """Check if an issue matches known false positive patterns"""
        
        # Extract file paths from locations
        file_paths = []
        for location in locations:
            physical_location = location.get('physicalLocation', {})
            artifact_location = physical_location.get('artifactLocation', {})
            uri = artifact_location.get('uri', '')
            if uri:
                file_paths.append(uri)
        
        # Check each pattern category
        for pattern_category, patterns in self.false_positive_patterns.items():
            for pattern in patterns:
                confidence = 0.0
                
                # Check rule pattern
                if 'rule_pattern' in pattern:
                    if re.search(pattern['rule_pattern'], rule_id, re.IGNORECASE):
                        confidence += 0.4
                
                # Check message pattern
                if 'message_pattern' in pattern:
                    if re.search(pattern['message_pattern'], message, re.IGNORECASE):
                        confidence += 0.3
                
                # Check file pattern
                if 'file_pattern' in pattern:
                    for file_path in file_paths:
                        if re.search(pattern['file_pattern'], file_path, re.IGNORECASE):
                            confidence += 0.3
                            break
                
                # If confidence is high enough, consider it a false positive
                if confidence >= 0.6:
                    return {
                        "reason": pattern['reason'],
                        "pattern_type": pattern_category,
                        "confidence": confidence
                    }
        
        return None
    
    def generate_report(self) -> str:
        """Generate a comprehensive false positive analysis report"""
        results = self.analyze_sarif_results()
        
        if "error" in results:
            return f"Analysis Error: {results['error']}"
        
        report = []
        report.append("=== AI-POWERED CODEQL FALSE POSITIVE ANALYSIS ===")
        report.append("")
        
        # Summary statistics
        total = results["total_issues"]
        fp = results["false_positives"]
        valid = results["valid_issues"]
        fp_rate = (fp / total * 100) if total > 0 else 0
        
        report.append(f"📊 ANALYSIS SUMMARY:")
        report.append(f"   Total issues found: {total}")
        report.append(f"   False positives: {fp}")
        report.append(f"   Valid issues: {valid}")
        report.append(f"   False positive rate: {fp_rate:.1f}%")
        report.append("")
        
        # Pattern breakdown
        if results["pattern_matches"]:
            report.append("🎯 FALSE POSITIVE PATTERNS DETECTED:")
            for pattern_type, count in results["pattern_matches"].items():
                report.append(f"   {pattern_type}: {count} issues")
            report.append("")
        
        # False positive details
        if results["false_positive_details"]:
            report.append("❌ FALSE POSITIVES (Safe to ignore):")
            for fp in results["false_positive_details"][:10]:  # Show top 10
                confidence_stars = "★" * int(fp["confidence"] * 5)
                report.append(f"   • {fp['rule_id']}: {fp['reason']} {confidence_stars}")
            
            if len(results["false_positive_details"]) > 10:
                report.append(f"   ... and {len(results['false_positive_details']) - 10} more")
            report.append("")
        
        # Valid issues
        if results["valid_issue_details"]:
            report.append("⚠️  VALID ISSUES (Require attention):")
            for issue in results["valid_issue_details"][:5]:  # Show top 5
                severity_icon = "🔴" if issue["severity"] == "error" else "🟡"
                report.append(f"   {severity_icon} {issue['rule_id']}: {issue['message']}")
            
            if len(results["valid_issue_details"]) > 5:
                report.append(f"   ... and {len(results['valid_issue_details']) - 5} more")
            report.append("")
        
        # Recommendations
        report.append("💡 RECOMMENDATIONS:")
        if fp_rate > 50:
            report.append("   • High false positive rate detected - typical for game development")
            report.append("   • Focus on valid issues for security and quality improvements")
        elif fp_rate > 20:
            report.append("   • Moderate false positive rate - review patterns for accuracy")
        else:
            report.append("   • Low false positive rate - most issues require attention")
        
        if valid > 0:
            report.append(f"   • Review {valid} valid issues for potential improvements")
        else:
            report.append("   • No critical issues found - code quality looks good!")
        
        report.append("")
        report.append("=== END ANALYSIS ===")
        
        return "\n".join(report)

def main():
    """Main entry point"""
    sarif_file = sys.argv[1] if len(sys.argv) > 1 else "codeql-results.sarif"
    
    if not os.path.exists(sarif_file):
        print(f"Error: SARIF file '{sarif_file}' not found")
        sys.exit(1)
    
    analyzer = CodeQLFalsePositiveAnalyzer(sarif_file)
    report = analyzer.generate_report()
    print(report)

if __name__ == "__main__":
    main()
